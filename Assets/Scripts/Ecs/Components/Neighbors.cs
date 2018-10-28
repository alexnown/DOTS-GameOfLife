namespace alexnown.Ecs.Components
{
    struct Neighbors 
    {
        public int LeftUp;
        public int Up;
        public int RightUp;
        public int Right;
        public int Left;
        public int LeftDown;
        public int Down;
        public int RightDown;

        public static Neighbors Calculate(int posX, int posY, int width, int length)
        {
            int arrayIndex = posY * width + posX;
            int indexTop = arrayIndex + width;
            if (indexTop >= length) indexTop -= length;
            int indexDown = arrayIndex - width;
            if (indexDown < 0) indexDown += length;
            int leftOffsetX = posX == 0 ? (width - 1) : -1;
            int rightOffsetX = posX == width - 1 ? (1 - width) : 1;

            return new Neighbors
            {
                LeftUp = indexTop + leftOffsetX,
                Up = indexTop,
                RightUp = indexTop + rightOffsetX,
                Left = arrayIndex + leftOffsetX,
                Right = arrayIndex + rightOffsetX,
                LeftDown = indexDown + leftOffsetX,
                Down = indexDown,
                RightDown = indexDown + rightOffsetX
            };
        }

        public override string ToString()
        {
            return $"[{LeftUp} {Up} {RightUp} , {Left} x {Right}, {LeftDown} {Down} {RightDown}]";
        }
    }
}
