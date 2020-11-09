# DOTS GameOfLife
Unity DOTS implementation for game of life.
### Features:
* Cells data stored in Texture RawData (no need addition copying cells state to texture, cells colored by custom shaders)
* Implemented two different cellular automaton algorithm:
1) Conway’s Game of Life (super optimized, 12 cells stored in one int, updated by vectorized bitwise operations)
2) More sustained algorithm, [named Steppers]
![Steppers game of life](https://i.ibb.co/xYrvHyz/steppers1k-2.png)

[named Steppers]:https://habr.com/ru/post/237629/
