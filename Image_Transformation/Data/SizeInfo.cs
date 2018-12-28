namespace Image_Transformation
{
    /// <summary>
    /// Holds information about the size of a matrix after a transformation.
    /// </summary>
    public struct SizeInfo
    {
        public int BiggestX { get; set; }
        public int BiggestY { get; set; }
        public int BiggestZ { get; set; }
        public int Depth { get; set; }
        public int Height { get; set; }
        public int SmallestX { get; set; }
        public int SmallestY { get; set; }
        public int SmallestZ { get; set; }
        public int Width { get; set; }
    }
}