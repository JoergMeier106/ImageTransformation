namespace Image_Transformation
{
    /// <summary>
    /// Holds information about an image.
    /// </summary>
    public struct ImageMetaInformation
    {
        public double BrightnessFactor { get; set; }
        public int BytePerPixel { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public static bool operator !=(ImageMetaInformation information1, ImageMetaInformation information2)
        {
            return !(information1 == information2);
        }

        public static bool operator ==(ImageMetaInformation information1, ImageMetaInformation information2)
        {
            return information1.Equals(information2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ImageMetaInformation))
            {
                return false;
            }

            var information = (ImageMetaInformation)obj;
            return Height == information.Height &&
                   Width == information.Width &&
                   BrightnessFactor == information.BrightnessFactor &&
                   BytePerPixel == information.BytePerPixel;
        }

        public override int GetHashCode()
        {
            var hashCode = -699679232;
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + BrightnessFactor.GetHashCode();
            hashCode = hashCode * -1521134295 + BytePerPixel.GetHashCode();
            return hashCode;
        }
    }
}