# DOTS GameOfLife
Unity DOTS implementation for game of life.
### Features:
1) Cells data allocated at BlobArray<byte>.
2) Used ConvertToEntity workflow, all simulation parameters are configured by GameOfLifeProxy script.
3) Used unsafe conversion BlobArray<byte> to NativeArray<byte> before scheduling job.
4) Simulation results drawn on screen by Graphics.DrawTexture in OnGUI method.
5) Implemented two different cellular automaton algorithm:
* Conway’s Game of Life
* More sustained algorithm, [named Steppers]
![Steppers game of life](https://i.ibb.co/xYrvHyz/steppers1k-2.png)

[named Steppers]:https://habr.com/ru/post/237629/
