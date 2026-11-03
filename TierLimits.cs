namespace SimyaBot;

public static class TierLimits
{
    public static int[] Defaults() => [2,2,2,2,2,2,2,2];
    public static void Validate(int[] limits,int capacity,int target)
    {
        if(target<1||target>7)throw new ArgumentException("Hedef Yontulmuş ile Tanrısal arasında olmalı.");
        if(limits==null||limits.Length!=8)throw new ArgumentException("Her kademe için bir dolu yuva limiti ayarla.");
        if(limits.Take(target).Any(n=>n<2||n>capacity))throw new ArgumentException($"İşlenecek kademelerin limitleri 2 ile {capacity} arasında olmalı.");
    }
    public static bool ShouldRefine(int tier,int target,int occupied,int[] limits) =>
        tier>=0&&tier<target&&tier<limits.Length&&occupied>=2&&occupied>=limits[tier];
    public static void SelfTest()
    {
        static void Assert(bool ok){if(!ok)throw new Exception("Kademe limiti testi başarısız.");}
        int[] limits=[25,12,8,6,4,2,2,2];Validate(limits,32,5);
        Assert(!ShouldRefine(0,5,24,limits));Assert(ShouldRefine(0,5,25,limits));Assert(ShouldRefine(0,5,32,limits));
        Assert(!ShouldRefine(1,5,11,limits));Assert(ShouldRefine(1,5,12,limits));
        Assert(!ShouldRefine(2,5,7,limits));Assert(ShouldRefine(2,5,8,limits));
        Assert(!ShouldRefine(5,5,32,limits));Assert(!ShouldRefine(6,5,32,limits));Assert(!ShouldRefine(7,7,32,limits));
        Assert(!ShouldRefine(5,7,1,limits));Assert(ShouldRefine(5,7,2,limits));
        foreach(var bad in new[]{new[]{1,2,2,2,2,2,2,2},new[]{33,2,2,2,2,2,2,2},new[]{2,2}}){bool rejected=false;try{Validate(bad,32,5);}catch(ArgumentException){rejected=true;}Assert(rejected);}
        var restored=System.Text.Json.JsonSerializer.Deserialize<Settings>(System.Text.Json.JsonSerializer.Serialize(new Settings{MinimumSlots=limits}))!;
        Assert(restored.MinimumSlots.SequenceEqual(limits));
        var legacy=System.Text.Json.JsonSerializer.Deserialize<Settings>("{\"Target\":5,\"PatternEnabled\":true,\"Pattern\":[{\"Tier\":0,\"Repeats\":2}]}")!;
        Assert(legacy.MinimumSlots.SequenceEqual(Defaults()));
        Assert(!System.Text.Json.JsonSerializer.Serialize(legacy).Contains("Pattern"));
    }
}
