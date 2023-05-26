# Animation Event Editor for FBX files
This editor tool was personally commissioned for the Elden ring series by Sebastian. This easily adds animation events to animation clips without needing to separate the clip from the FBX file and is a more intuitive way.

## Current Features
* Load animation clips from FBX 
* Add/Edit/Remove animation events from clips
* Can preview animations on in-scene models and an in-built previewer
* Search tree of methods for method name

## How to use
* You can select an FBX file with animation clips in your project files or you can manually drag it into the "Animation FBX" field.
* The "In Scene Model" field is optional and will allow for animation previews of the targeted model in the unity scene (Note does not return to the original pose.)
* A list of animation clips will appear once an FBX file is selected, once an animation clip is selected you can then add animation events using the "+" button.
* Search tree works by finding methods from getting a gameObject with the animator component placed in "In Scene Model" otherwise if empty no method names will appear
* The animation controls above allow for animation previewing and when creating an animation event it will use the slider's current value for the new animation event.
* Once done remember to click save to ensure the changes are applied. Note that changing animation clips does not remove your events from the previous animation clips so you can continue editing.

![image](https://raw.githubusercontent.com/InsomniacSnorlax/FBX-Animation-Event/main/img/v1.1.0%20Image%201.png)
![image](https://raw.githubusercontent.com/InsomniacSnorlax/FBX-Animation-Event/main/img/v1.1.0%20Image%202.png)

## Planned Features
* Collapsable List
* ~~In-window animation previewer (in scene alternative)~~
* Change slider
* Replicate Unity's animation timeline
* Use Animator Controller to get animation clips (Already done just lazy)
* Ability to edit single animation clips (Already working on)
* Edit animation clips more (Name, Curves, Settings etc)
