namespace SimyaBot;

public class LimitsEditor : Form
{
    public int[] Limits {get;private set;}
    public LimitsEditor(string[] tiers,int target,int capacity,int[] limits)
    {
        Limits=(int[])limits.Clone();
        Text="Kademe başına dolu yuva limiti";ClientSize=new(510,475);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;StartPosition=FormStartPosition.CenterParent;
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new(16),ColumnCount=2,RowCount=11};
        layout.ColumnStyles.Add(new(SizeType.Percent,65));layout.ColumnStyles.Add(new(SizeType.Percent,35));Controls.Add(layout);
        layout.RowStyles.Add(new(SizeType.Absolute,65));
        var info=new Label{Dock=DockStyle.Fill,Text=$"Limit dahil: 25 seçersen 25 ve üzeri dolu yuvada arındırır. Altındaysa taşları biriktirip sonraki kademeye geçer.\nIzgara: {capacity} yuva. Hedef: {tiers[target]} (işlenmez)."};layout.Controls.Add(info,0,0);layout.SetColumnSpan(info,2);
        var inputs=new NumericUpDown[8];
        for(int i=0;i<8;i++){
            layout.RowStyles.Add(new(SizeType.Absolute,35));bool active=i<target;
            layout.Controls.Add(new Label{AutoSize=true,Anchor=AnchorStyles.Left,Text=tiers[i]+(active?"":" • işlenmez")},0,i+1);
            inputs[i]=new NumericUpDown{Minimum=2,Maximum=Math.Max(2,capacity),Value=Math.Clamp(limits.ElementAtOrDefault(i),2,Math.Max(2,capacity)),Enabled=active,Width=100,Anchor=AnchorStyles.Left};layout.Controls.Add(inputs[i],1,i+1);
        }
        var note=new Label{Dock=DockStyle.Fill,Text="Limitler tüm renklerde geçerlidir. Sayı, taş yığını adedi değil dolu yuva sayısıdır. Hedefi değiştirince o kademenin limiti devreye girer."};layout.Controls.Add(note,0,9);layout.SetColumnSpan(note,2);layout.RowStyles.Add(new(SizeType.Absolute,55));
        var buttons=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft};layout.Controls.Add(buttons,0,10);layout.SetColumnSpan(buttons,2);layout.RowStyles.Add(new(SizeType.Percent,100));
        var save=new Button{Text="Kaydet",Width=100,Height=30};var cancel=new Button{Text="İptal",Width=100,Height=30,DialogResult=DialogResult.Cancel};buttons.Controls.Add(save);buttons.Controls.Add(cancel);CancelButton=cancel;
        save.Click+=(_,_)=>{Limits=inputs.Select(n=>(int)n.Value).ToArray();DialogResult=DialogResult.OK;Close();};
    }
}
