![Unity Experiment Framework](media/banner.png)

# UXF - Unity Experiment Framework
A set of Unity components which simplify management of human-based experiments developed in Unity.


# Get started

1. Import the latest ```UXF.unitypackage``` [release](https://github.com/jackbrookes/unity-experiment-framework/releases) to your existing Unity project.

2. In Unity, go to `Edit` -> `Player` -> `Other` and change API Compatibility Level to .NET 2.0 in Unity player settings. 

3. Open an example scene or add the `[UXF_Rig]` prefab to your scene.

4. Press play, and use the UI to create a new participant list in an appropriate directory.

5. Press start to begin the session.


# Background

## Why?

Unity is becoming increasingly popular in human behaviour research as a means of easily building experiments with rich visuals and powerful interaction mechanisms. Additionally, Unity is the consistently the first compatibility target for newly available VR & AR devices and their accompanying features. 

Developing experiments usually contains two parts.

1. Creating the mechanics of the experiment: Creating the objects, stimuli, feedback mechanisms, and how the user interacts with these things (*the fun part*).

2. Wrapping an experiment around the task: Recording user responses, building systems for participant information collection, configuration, file reading a writing (*the less fun part*).

The second part is daunting for new programmers, and can create hiding places for bugs. UXF provides a set of systems which perform most of these tasks, but does not interfere with or restrict the mechanics of the experiment in any way.

## Who is this for?

This framework is aimed at people who are already at least somewhat familar with creating games or experiments in Unity. It will make development of tasks faster and easier, and also add some useful additional features to your experiments. It does not take away the requirement for programming when developing an experiment. It also does not add any stimulus presentation features, as Unity already provides everything someone would need. In fact, this framework is completely agnostic to all presentation mechanisms, and works with 2D, 3D and VR experiments. 


## Compatibility

This package is tested with Windows and works with Unity 2017 or later.
