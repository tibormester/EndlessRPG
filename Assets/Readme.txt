Version at least 9?
Starting from scratch again, this time with Unity.
Diving into Godot's GDExtension wasn't very fun and GDScript is a pain to work with, so I decided to look at unity.
Other than longer compile times, Unity seems to be better in every aspect, more documentation, more reasonable APIs, better editor, VSCode Integration...
Starting from scratch and getting used to Unity has set me back quite a bit, but it's not like I was too deep into my project anyways.
I practiced a little with runtime mesh generation and editing, getting it to deform elastically on click, but I have been struggling to edit specific triangles at the point of impact
The approach i was using also required a mesh collider which isn't a very scalable solution so I think I will instead go for a different solution.
Putting that aspect of the game on pause, I have been trying to figure out the best way to utilize the Input System. After a few iterations, I have settled on using C# events through an InputActions Asset
I have also Structured the character to have a character controller and each movment action is a separate component that moves the character.
I still need to implement the core combat features of aimed slashes and thrusts as well as implementing interacting with objects, but I thought to first develop the character itself further
This is kind of crucial because things like arm length and shoulder height while determine the key frames for the swinging animations, but looking into this I'm thinking I might want to swap my character controller for a rigidbody
With a character controller I don't seem to be able to use joints, and although I can probably find a work around, I am not using much from the character controller that a rigid body can't offer
So my next step will be swapping from a character controller to a rigidbody, reworking any of my movment actions that need it... Reimplementing character controller built in code like slope detection
I watched a video of implementing smooth toy-like movement by using a floating capsule approach to character movement and I think it would work pretty well for my application as the character's root node
and all the child bones created by the joint attached rigid bodies will be asleep and controlled by active animations until an appropriate collision forces some ragdolling or active ragdolling to occour...

Once that is all done then hopefully I will have a playable movement + combat system, then I will implement health + stats and enemy AI.... Which will lead to looting and crafting and upgrades etc....
And then eventually I can finally work on my procedural island generation that I really want the game to be about and then maybe I can redo everything to add multiplayer