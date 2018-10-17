namespace Image_Transformation
{
    public struct ImageMetaInformation
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public double BrightnessFactor { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is ImageMetaInformation))
            {
                return false;
            }

            var information = (ImageMetaInformation)obj;
            return Height == information.Height &&
                   Width == information.Width &&
                   BrightnessFactor == information.BrightnessFactor;
        }

        public override int GetHashCode()
        {
            var hashCode = 2044915702;
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + BrightnessFactor.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ImageMetaInformation information1, ImageMetaInformation information2)
        {
            return information1.Equals(information2);
        }

        public static bool operator !=(ImageMetaInformation information1, ImageMetaInformation information2)
        {
            return !(information1 == information2);
        }
    }
}