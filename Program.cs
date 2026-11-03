using System.Runtime.InteropServices;
using System.Text.Json;

namespace SimyaBot;

static class Program
{
    [STAThread] static void Main(string[] args) {
        if(args.Contains("--self-test")){SlotDetector.SelfTest();TierLimits.SelfTest();return;}
        ApplicationConfiguration.Initialize(); Application.Run(new MainForm());
    }
}

public record Position(int X, int Y) { public Point Point => new(X,Y); }
public class Settings
{
    public Dictionary<string, Position> Points { get; set; } = new();
    public Dictionary<string, int[]> Empty { get; set; } = new();
    public int Target { get; set; } = 5;
    public int Delay { get; set; } = 500;
    public int Spacing { get; set; } = 32;
    public int Columns { get; set; } = 8;
    public int Rows { get; set; } = 4;
    public int Threshold { get; set; } = 28;
    public bool[] Colors { get; set; } = [true,true,true,true,true,true,false];
    public int[] MinimumSlots { get; set; } = TierLimits.Defaults();
}

public class MainForm : Form
{
    static readonly string[] Colors = ["Beyaz", "Mavi", "Yeşil", "Siyah", "Turuncu", "Kırmızı", "Mor"];
    static readonly string[] Tiers = ["İşlenmemiş", "Yontulmuş", "Ender", "Antika", "Efsanevi", "Mitsi", "Destansı", "Tanrısal"];
    readonly string config = Path.Combine(AppContext.BaseDirectory, "ayarlar.json");
    Settings settings = new();
    readonly ComboBox target = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width=150 };
    readonly CheckedListBox colors = new() { Height=125, Width=160, CheckOnClick=true };
    readonly ListBox points = new() { Dock=DockStyle.Fill };
    readonly TextBox log = new() { Multiline=true, ReadOnly=true, ScrollBars=ScrollBars.Vertical, Dock=DockStyle.Fill };
    readonly NumericUpDown delay = Number(150,5000,500), spacing = Number(16,100,32), columns = Number(1,16,8), rows = Number(1,12,4), threshold=Number(5,150,28);
    readonly System.Windows.Forms.Timer keys = new() { Interval=40 };
    CancellationTokenSource? cancellation;
    string? capture;
    bool lastF6, lastF8;
    nint game;
    public MainForm()
    {
        Text="Osi2 Simya • Ekran Otomasyonu"; Width=900; Height=740; MinimumSize=new(850,680);
        try { if(File.Exists(config)) settings=JsonSerializer.Deserialize<Settings>(File.ReadAllText(config)) ?? new(); } catch { }
        target.Items.AddRange(Tiers); target.SelectedIndex=Math.Clamp(settings.Target,1,7);
        for(int i=0;i<Colors.Length;i++) colors.Items.Add(Colors[i],settings.Colors.ElementAtOrDefault(i));
        delay.Value=Math.Clamp(settings.Delay,150,5000); spacing.Value=Math.Clamp(settings.Spacing,16,100);
        columns.Value=Math.Clamp(settings.Columns,1,16); rows.Value=Math.Clamp(settings.Rows,1,12); threshold.Value=Math.Clamp(settings.Threshold,5,150);
        var root=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=2, RowCount=3, Padding=new(12) };
        root.ColumnStyles.Add(new(SizeType.Percent,45));root.ColumnStyles.Add(new(SizeType.Percent,55));
        root.RowStyles.Add(new(SizeType.Absolute,215));root.RowStyles.Add(new(SizeType.Percent,55));root.RowStyles.Add(new(SizeType.Percent,45)); Controls.Add(root);
        var options=new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown, WrapContents=false };
        options.Controls.Add(new Label { Text="İşlenecek renkler", AutoSize=true }); options.Controls.Add(colors);
        options.Controls.Add(new Label { Text="Hedef kademe (bu kademe işlenmez)", AutoSize=true });options.Controls.Add(target);root.Controls.Add(options,0,0);
        var tuning=new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown, WrapContents=false };
        AddField(tuning,"İşlem beklemesi (ms)",delay); AddField(tuning,"Yuva merkezleri arası (px)",spacing);
        AddField(tuning,"Sütun / satır",columns,rows);AddField(tuning,"Görüntü farkı eşiği",threshold);
        tuning.Controls.Add(new Label { AutoSize=true, MaximumSize=new(420,70),Text="Kalibrasyon: listeden noktayı seç → Konum ata → oyun üzerinde fareyi götür → F6. Boş örnek, tamamen boş simya ızgarasında alınmalı." }); root.Controls.Add(tuning,1,0);
        root.Controls.Add(points,0,1);
        foreach(var c in Colors) {points.Items.Add("Sandık: "+c);points.Items.Add("Renk: "+c);}
        foreach(var t in Tiers) points.Items.Add("Kademe: "+t);
        points.Items.Add("Izgara ilk yuva"); points.Items.Add("Hızlı Arındır");points.Items.Add("Arındırma giriş yuvası"); points.SelectedIndex=0;
        points.DrawMode=DrawMode.OwnerDrawFixed;points.DrawItem+=(s,e)=>{if(e.Index<0)return;e.DrawBackground();string key=(string)points.Items[e.Index];string suffix=settings.Points.TryGetValue(key,out var p)?$"  ✓ {p.X},{p.Y}":"  • eksik";e.Graphics.DrawString(key+suffix,e.Font!,new SolidBrush(e.ForeColor),e.Bounds);};
        var actions=new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.TopDown, WrapContents=false, AutoScroll=true };
        Button Add(string title,Action action) {var b=new Button {Text=title,Width=330,Height=34};b.Click+=(_,_)=>{try{action();}catch(Exception ex){MessageBox.Show(ex.Message);}};actions.Controls.Add(b);return b;}
        Add("Seçili noktaya konum ata (F6)",()=>{EnsureIdle();capture=(string)points.SelectedItem!;Write(capture+": fareyi götür ve F6 bas.");});
        Add("Izgara başlangıcını Beyaz sekmesine hizala",()=>{
            EnsureIdle();var anchor=P("Renk: Beyaz");settings.Points["Izgara ilk yuva"]=new(anchor.X+4,anchor.Y+80);settings.Empty.Clear();Save();points.Invalidate();
            Write("Izgara ilk yuvası Beyaz sekmesine göre hizalandı. Bu yerleşim için +4,+80 kullanıldı. İlk yuva merkezini kontrol et; sonra BOŞ sayfada boş örneği yeniden al.");
            MessageBox.Show(this,"İlk yuva Beyaz renk sekmesinden 4 piksel sağa, 80 piksel aşağıya ayarlandı.\nİlk yuva merkezini kontrol et. Gerekirse F6 ile düzelt.\nArdından tamamen BOŞ bir simya sayfasında boş örneği yeniden al.","Izgara kalibrasyonu");
        });
        Add("Boş ızgara örneği al (3 sn sonra)",async()=>{try{EnsureIdle();ReadOptions();Write("Izgara tamamen boş olsun. 3 saniye...");await Task.Delay(3000);CaptureEmpty();Save();Write("Boş yuva örnekleri kaydedildi.");}catch(Exception ex){Write(ex.Message);}});
        Add("Yuva sayımını dene (3 sn sonra)",async()=>{try{EnsureIdle();ReadOptions();await Task.Delay(3000);Native.Move(P("Hızlı Arındır").Point);await Task.Delay(300);Write("Algılanan taş: "+Occupied(true).Count);}catch(Exception ex){Write(ex.Message);}});
        Add("Ayarları kaydet",()=>{EnsureIdle();ReadOptions();Save();Write("Ayarlar kaydedildi.");});
        Add("Kademe doluluk limitleri",()=>{
            EnsureIdle();ReadOptions();using var editor=new LimitsEditor(Tiers,settings.Target,settings.Columns*settings.Rows,settings.MinimumSlots);
            if(editor.ShowDialog(this)==DialogResult.OK){settings.MinimumSlots=editor.Limits;Save();Write("Kademe limitleri kaydedildi: "+string.Join(", ",Tiers.Take(settings.Target).Select((t,i)=>$"{t} ≥{settings.MinimumSlots[i]}")));}
        });
        Add("Seçili konuma hareket testi (tıklamaz)",TestMovement);
        Add("BAŞLAT (3 sn sonra)",Start);
        Add("DURDUR • F8",Stop);
        root.Controls.Add(actions,1,1);root.Controls.Add(log,0,2);root.SetColumnSpan(log,2);
        keys.Tick+=(_,_)=>{bool f6=Native.Down(0x75),f8=Native.Down(0x77);if(f8&&!lastF8)Stop();if(f6&&!lastF6&&capture!=null){var p=Cursor.Position;settings.Points[capture]=new(p.X,p.Y);Write(capture+" kaydedildi.");capture=null;points.Invalidate();Save();}lastF6=f6;lastF8=f8;}; keys.Start();
        FormClosing+=(_,_)=>Stop();Write("Önce konumları ve boş ızgarayı kalibre et. F8 her zaman durdurur.");
    }
    static NumericUpDown Number(int min,int max,int value)=>new(){Minimum=min,Maximum=max,Value=value,Width=80};
    static void AddField(Control parent,string text,params Control[] controls){var p=new FlowLayoutPanel{Width=420,Height=30};p.Controls.Add(new Label{Text=text,Width=210});p.Controls.AddRange(controls);parent.Controls.Add(p);}
    void Write(string text){if(!IsDisposed){var line=$"[{DateTime.Now:HH:mm:ss}] {text}\r\n";log.AppendText(line);Text="Osi2 Simya • "+text;try{File.AppendAllText(Path.Combine(AppContext.BaseDirectory,"bot.log"),line);}catch{}}}
    void EnsureIdle(){if(cancellation!=null)throw new Exception("Önce botu durdur.");}
    void ReadOptions(){settings.Target=target.SelectedIndex;settings.Delay=(int)delay.Value;settings.Spacing=(int)spacing.Value;settings.Columns=(int)columns.Value;settings.Rows=(int)rows.Value;settings.Threshold=(int)threshold.Value;settings.Colors=Enumerable.Range(0,Colors.Length).Select(colors.GetItemChecked).ToArray();}
    void Save()=>File.WriteAllText(config,JsonSerializer.Serialize(settings,new JsonSerializerOptions{WriteIndented=true}));
    Position P(string key)=>settings.Points.TryGetValue(key,out var p)?p:throw new Exception("Eksik konum: "+key);
    Point Cell(int i){var p=P("Izgara ilk yuva");return new(p.X+i%settings.Columns*settings.Spacing,p.Y+i/settings.Columns*settings.Spacing);}
    int[] Sample(int i)=>SampleAt(Cell(i));
    int[] SampleAt(Point p){using var bmp=new Bitmap(18,18);using(var g=Graphics.FromImage(bmp))g.CopyFromScreen(p.X-9,p.Y-9,0,0,bmp.Size);var values=new List<int>();for(int y=0;y<18;y+=2)for(int x=0;x<18;x+=2){var c=bmp.GetPixel(x,y);values.Add(c.R);values.Add(c.G);values.Add(c.B);}return values.ToArray();}
    void CaptureEmpty(){settings.Empty.Clear();for(int i=0;i<settings.Columns*settings.Rows;i++)settings.Empty[i.ToString()]=Sample(i);}
    List<int> Occupied(bool diagnostics=false){var result=new List<int>();var scores=new List<string>();for(int i=0;i<settings.Columns*settings.Rows;i++){if(!settings.Empty.TryGetValue(i.ToString(),out var empty))throw new Exception("Izgara boş örneğini yeniden al.");double diff=SlotDetector.Score(Sample(i),[empty]);if(diff>=settings.Threshold)result.Add(i);scores.Add($"{i+1}:{diff:F1}");}if(diagnostics)Write("Yuva farkları (eşik "+settings.Threshold+"): "+string.Join(" ",scores));return result;}
    async Task<List<int>> ReadStones(CancellationToken token)
    {
        Check(token);Native.Move(P("Hızlı Arındır").Point);await Task.Delay(300,token);
        var votes=new Dictionary<int,int>();
        for(int n=0;n<3;n++){Check(token);foreach(var i in Occupied())votes[i]=votes.GetValueOrDefault(i)+1;if(n<2)await Task.Delay(120,token);}
        return votes.Where(p=>p.Value>=2).Select(p=>p.Key).Order().ToList();
    }
    async Task<(bool Added,List<int> Stones)> FillChest(int color,List<int> before,CancellationToken token)
    {
        var current=before;
        for(int attempt=1;attempt<=3;attempt++){
            Write($"{Colors[color]}: Ctrl + sağ tık, deneme {attempt}/3.");
            await Click("Sandık: "+Colors[color],true,token,true);
            current=await ReadStones(token);Write($"Sandık sonrası algılanan taş: {current.Count} (önce {before.Count}).");
            if(current.Count>before.Count)return(true,current);
        }
        Write(Colors[color]+": 3 denemede yeni taş gelmedi; bu rengin sandığı bitmiş kabul ediliyor.");
        return(false,current);
    }
    void Stop(){cancellation?.Cancel();capture=null;}
    async Task<bool> SelectStone(List<int> occupied,CancellationToken token)
    {
        // Compare the refinement input before/after each selection. Empty clicks
        // cannot trigger Quick Refine without a visible input change.
        var candidates=occupied.OrderByDescending(i=>SlotDetector.Score(Sample(i),[settings.Empty[i.ToString()]])).Take(3).ToArray();
        foreach(var index in candidates){
            var confirmed=await ReadStones(token);if(!confirmed.Contains(index))continue;
            var before=SampleAt(P("Arındırma giriş yuvası").Point);
            await ClickAt(Cell(index),true,token);
            Native.Move(P("Hızlı Arındır").Point);await Task.Delay(250,token);Check(token);
            var after=SampleAt(P("Arındırma giriş yuvası").Point);
            if(SlotDetector.Score(after,[before])>=Math.Max(12,settings.Threshold*0.7))return true;
            Write($"Yuva {index+1}: arındırma girişinde değişim yok; Hızlı Arındır gönderilmedi.");
        }
        return false;
    }
    async void Start()
    {
        if(cancellation!=null)return;
        var cts=new CancellationTokenSource();cancellation=cts;
        try{
            capture=null;ReadOptions();
            if(!settings.Colors.Any(x=>x))throw new Exception("En az bir renk seç.");
            TierLimits.Validate(settings.MinimumSlots,settings.Columns*settings.Rows,settings.Target);
            for(int c=0;c<Colors.Length;c++)if(settings.Colors[c]){P("Sandık: "+Colors[c]);P("Renk: "+Colors[c]);}
            for(int t=0;t<=settings.Target;t++)P("Kademe: "+Tiers[t]);
            P("Hızlı Arındır");P("Izgara ilk yuva");P("Arındırma giriş yuvası");
            if(settings.Empty.Count!=settings.Rows*settings.Columns)throw new Exception("Boş ızgara örneğini al.");Save();
            Write("3 saniye içinde oyuna geç. Her kademe ekrandan sayılacak; arındırma giriş yuvası boş olmalı.");await Task.Delay(3000,cts.Token);
            game=Native.GetForegroundWindow();if(game==Handle||game==0)throw new Exception("Oyun penceresi önde olmalı.");
            for(int c=0;c<Colors.Length;c++){if(!settings.Colors[c])continue;
                await Click("Renk: "+Colors[c],false,cts.Token);
                bool exhausted=false;int stalledRounds=0;
                while(!exhausted){
                    Check(cts.Token);bool progress=false,attempted=false;
                    await Click("Kademe: "+Tiers[0],false,cts.Token);
                    var raw=await ReadStones(cts.Token);
                    if(!TierLimits.ShouldRefine(0,settings.Target,raw.Count,settings.MinimumSlots)){
                        var filled=await FillChest(c,raw,cts.Token);exhausted=!filled.Added;progress=filled.Added;
                    }
                    for(int t=0;t<settings.Target;t++){
                        await Click("Kademe: "+Tiers[t],false,cts.Token);
                        var occupied=await ReadStones(cts.Token);
                        Write($"{Colors[c]} / {Tiers[t]}: {occupied.Count} dolu yuva, limit ≥{settings.MinimumSlots[t]}.");
                        if(!TierLimits.ShouldRefine(t,settings.Target,occupied.Count,settings.MinimumSlots))continue;
                        attempted=true;
                        if(!await SelectStone(occupied,cts.Token)){
                            Write("Dolu taşın panele aktarımı doğrulanamadı; bu kademe atlandı.");continue;
                        }
                        await Click("Hızlı Arındır",false,cts.Token);
                        await Task.Delay(settings.Delay,cts.Token);
                        var remaining=await ReadStones(cts.Token);
                        if(remaining.Count>=occupied.Count){
                            // Animation and server response can outlast the first reading.
                            await Task.Delay(Math.Max(800,settings.Delay),cts.Token);remaining=await ReadStones(cts.Token);
                        }
                        if(remaining.Count<occupied.Count){progress=true;Write($"Arındırma sonrası {remaining.Count} yuva algılandı.");}
                        else Write("Sayımda azalma doğrulanamadı; aynı kademede yeniden arındırmadan sonraki kademeye geçiliyor.");
                    }
                    stalledRounds=progress?0:stalledRounds+1;
                    if(!exhausted&&attempted&&stalledRounds>=3)throw new Exception("Üç tam turda ilerleme doğrulanamadı. İşlem durduruldu. Boş sayfa sayımının 0 olduğunu ve arındırma giriş yuvasının konumunu kontrol et.");
                }
                Write(Colors[c]+": sandıktan yeni taş gelmiyor; sonraki renge geçiliyor.");
            }
            Write("Seçili renkler tamamlandı. Hedef kademe ve limit altındaki taşlar korundu.");
        }catch(OperationCanceledException){Write("Durduruldu.");}catch(Exception ex){Write("Durdu: "+ex.Message);MessageBox.Show(this,ex.Message,"Bot durdu",MessageBoxButtons.OK,MessageBoxIcon.Warning);}finally{cancellation=null;cts.Dispose();}
    }
    async void TestMovement()
    {
        if(cancellation!=null)return;
        var cts=new CancellationTokenSource();cancellation=cts;
        try {
            var key=(string)points.SelectedItem!;var destination=P(key).Point;
            Write("Hareket testi: 3 saniye içinde oyuna geç. Tıklama yapılmayacak; F8 iptal.");
            await Task.Delay(3000,cts.Token);game=Native.GetForegroundWindow();
            if(game==Handle||game==0)throw new Exception("Oyun penceresine geçmedin.");
            Write("Test hedefi: "+Native.Title(game));Check(cts.Token);Native.Move(destination);
            await Task.Delay(250,cts.Token);Check(cts.Token);var actual=Cursor.Position;
            Write($"Hareket testi: hedef {destination.X},{destination.Y}; Windows imleci {actual.X},{actual.Y}.");
            if(Math.Abs(actual.X-destination.X)>4||Math.Abs(actual.Y-destination.Y)>4)
                throw new Exception("İmleç hedefe ulaşmadı veya oyun tarafından geri taşındı. Oyun pencere modunda olmalı. Oyun yönetici olarak çalışıyorsa botu da aynı yetki düzeyinde aç.");
        }catch(OperationCanceledException){Write("Test iptal edildi.");}
        catch(Exception ex){Write("Test: "+ex.Message);MessageBox.Show(this,ex.Message,"Hareket testi");}
        finally{cancellation=null;cts.Dispose();}
    }
    void Check(CancellationToken token){token.ThrowIfCancellationRequested();if(Native.GetForegroundWindow()!=game)throw new Exception("Oyun odağı kayboldu.");}
    new Task Click(string key,bool right,CancellationToken token,bool ctrl=false)=>ClickAt(P(key).Point,right,token,ctrl);
    async Task ClickAt(Point p,bool right,CancellationToken token,bool ctrl=false){Check(token);Native.Move(p);await Task.Delay(150,token);Check(token);try{if(ctrl){Native.Key(true);await Task.Delay(350,token);Check(token);if(!Native.Down(0xA2))throw new Exception("Sol Ctrl basılı durumu doğrulanamadı; sandığa tıklanmadı.");Write("Sol Ctrl basılı doğrulandı; sağ tık gönderiliyor.");}Native.Mouse(right,true);await Task.Delay(ctrl?150:60,token);}finally{try{Native.Mouse(right,false);if(ctrl)await Task.Delay(180);}finally{if(ctrl)Native.Key(false);}}await Task.Delay(settings.Delay,token);}
}

static class Native
{
    [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] public static extern nint GetForegroundWindow();
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(nint window,System.Text.StringBuilder text,int length);
    [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint count,INPUT[] inputs,int size);
    [StructLayout(LayoutKind.Sequential)] struct INPUT {public uint type; public DATA data;}
    [StructLayout(LayoutKind.Explicit)] struct DATA {[FieldOffset(0)]public MOUSE mouse;[FieldOffset(0)]public KEY key;}
    [StructLayout(LayoutKind.Sequential)] struct MOUSE {public int x,y; public uint data,flags,time; public nuint extra;}
    [StructLayout(LayoutKind.Sequential)] struct KEY {public ushort vk,scan;public uint flags,time;public nuint extra;}
    public static bool Down(int key)=>(GetAsyncKeyState(key)&0x8000)!=0;
    public static string Title(nint window){var text=new System.Text.StringBuilder(256);GetWindowText(window,text,text.Capacity);return text.ToString();}
    public static void Move(Point point)
    {
        var screen=SystemInformation.VirtualScreen;
        if(!screen.Contains(point))throw new Exception("Kaydedilmiş konum ekranın dışında. Konumları yeniden kalibre et.");
        int x=(int)Math.Round((point.X-screen.Left)*65535.0/Math.Max(1,screen.Width-1));
        int y=(int)Math.Round((point.Y-screen.Top)*65535.0/Math.Max(1,screen.Height-1));
        Send(new INPUT{type=0,data=new DATA{mouse=new MOUSE{x=x,y=y,flags=0x8000|0x4000|0x0001}}});
    }
    public static void Key(bool down)=>Send(new INPUT{type=1,data=new DATA{key=new KEY{scan=0x1D,flags=0x0008u|(down?0u:2u)}}});
    public static void Mouse(bool right,bool down)=>Send(new INPUT{type=0,data=new DATA{mouse=new MOUSE{flags=right?(down?8u:16u):(down?2u:4u)}}});
    static void Send(INPUT input){if(SendInput(1,[input],Marshal.SizeOf<INPUT>())!=1)throw new Exception($"Windows girdiyi kabul etmedi (hata {Marshal.GetLastWin32Error()}). Oyun yönetici olarak çalışıyorsa botu da aynı yetki düzeyinde aç. Ayrıntı: bot.log.");}
}
