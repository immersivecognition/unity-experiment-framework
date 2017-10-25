![Experiment manager for Unity](media/banner.png)

# Unity Experiment Manager
A set of C# scripts which simplifies management of human-based experiments developed in Unity. This is the example project, see Releases for latest Unity Package.

## Features

* Classes for common experimental concepts such as ```Session```, ```Block``` & ```Trial```
* Saves behavioural data to ```.CSV``` file, automatically handling file & directory naming
* Allows for tracking of position/rotation of any objects in the scene and writes to ```.CSV```
* Writes files in a seperate thread to avoid frame drops
* Cascading settings system, allowing setting independent variables at a Session, Block, or Trial level.
* Serialises and saves information to ```JSON``` file
* Helps create maintaiable code using an Object-Oriented Programming style

## Example

See ```Assets/ExpMngr/TestScript.cs``` for a simple example.

## Documentation

http://jackbrookes.github.io/unity-experiment-manager

## Usage

Download the project and open in Unity. Alternatively import the ```.unitypackage``` file to your existing Unity project.

## Future additions

* In experiment UI to monitor collection of data in real-time
* Recording of an experiment via an in-game camera (useful for mixed reality, and watching back afterwards)
