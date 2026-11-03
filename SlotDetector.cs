namespace SimyaBot;

public static class SlotDetector
{
    // Retain local differences made by small icons and animated glow. The caller
    // supplies the same slot's baseline, so another slot cannot mask a stone.
    public static double Score(int[] sample, IEnumerable<int[]> emptySamples)
    {
        double best=double.MaxValue;
        foreach(var empty in emptySamples)
        {
            if(empty.Length!=sample.Length||sample.Length==0)continue;
            var differences=new double[sample.Length/3];
            for(int p=0;p<differences.Length;p++)
                differences[p]=(Math.Abs(sample[p*3]-empty[p*3])+Math.Abs(sample[p*3+1]-empty[p*3+1])+Math.Abs(sample[p*3+2]-empty[p*3+2]))/3.0;
            Array.Sort(differences);
            int count=Math.Max(1,differences.Length/4);
            best=Math.Min(best,differences.TakeLast(count).Average());
        }
        if(best==double.MaxValue)throw new InvalidOperationException("Geçerli boş yuva örneği yok; boş örneği yeniden al.");
        return best;
    }
    public static void SelfTest()
    {
        var empty=Enumerable.Repeat(20,243).ToArray();
        var other=Enumerable.Repeat(60,243).ToArray();
        var small=(int[])empty.Clone();for(int i=0;i<36;i++)small[i]=120;
        if(Score(empty,[empty,other])!=0||Score(other,[empty,other])!=0)throw new Exception("Boş yuva eşleştirme hatası.");
        if(Score(small,[empty,other])<28)throw new Exception("Küçük taş algılama hatası.");
        var noise=empty.Select(v=>v+3).ToArray();if(Score(noise,[empty,other])>=28)throw new Exception("Gürültü boş yuva sayılmadı.");
    }
}
