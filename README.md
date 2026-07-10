# OpeNetLib
Lightweight UDP-based networking library built for games built on the .NET CLR.

# Why?
VoxelEngine needed a networking library, so I decided to write my own.

# Why would I use it?
In its current form, it is not ready for actual use.
It hasn't been tested over network, just locally.
At the moment I advise you _not_ to use it.

# What are its dependencies?
It runs on .NET Core 10.0, and uses no external dependencies.

# Notes
While OpeNetLib does come with a small in-house binary serialization framework, it has been deprecated.
It is recommended that you use an external library, which is what I built [Plancake](https://github.com/MonkeManII/Plancake) for.
