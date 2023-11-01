V1.1.0
Vision:
	No Man's Sky's Scale and Diversity: ADVENTURE
		infinite world, everything is procedurally generated and animated
	Kenshi's Complexity and Modularity: PROGRESSION
		various progression systems coupled with a broad set of gameplay systems with plenty of depth
	Warframe's Movement and Combat: FUN
		fast and smooth combat that is intuitive and simple to learn but with enough depth for skill and strategic mastery

Why have I restarted, again...?
I wanted to utilize an ECS system, but have strong OOP roots and inheritance is just too OP.
Additionally I started tyring to form an MVP without a plan, so I decided to restart with just developing the model.
After considering how to implement Entities, Containers, and Systems with a lot of bending the rules, using too much inheritance, and realizing that GDScripts inability to allow multi-classing or at least interfaces, all contributed to making the idea seem futile.

Looking for a Cleaner more Elegant Approach:
The real issue was in trying to implement modular characters and looking towards enabling them to be procedurally generated, makes me want to shy away from implementing any model specific animations...
This means the obvious solution is procedural animation (Both forward, inverse, and any other arbitrary ruleset that adds the right feeling to the motion)
I love the idea of procedural animation but thought it would be a huge task and just be scope creep keeping me from accomplishing my game, afterall it is only 1/3 of the core pillars and arguably the least important to how the game feels if not done properly (See No Man's Sky's inital failure)
However, on further consideration, procedural generation and animation are crucial to modularity, progression, and combat; so, any half baked solution will simply mean redoing the entire codebase after the Minimum Viable Product is finished.

Implementation:
	I will be utilizing GDExtension API to create base game classes and probably logic too in C++. Not only is the language compilable but is easier to use for polymorphic behavior which is honestly the easiest way to implement my take on a robust entity component systems from my perspective.
	I do like the idea of their being Entities and Components where Entities have Components and Systems act on the components. The advantage of OOP is the polymorphic behavior for components based on the type of entity as well as code reusability through inheritance for a lot of similar components however...
	More importantly OOP enables a recursive ECS behavior where components are also entities. This was probably my biggest headache in trying to diagram the previous iteration of this game: figuring out how an ability could be modular ensured that it couldn't be a component, however being able to slot an ability into a body part gave it a component like role....
	The Game will still have a model view / view-model separation with entities modeled by Signed Distance Functions (SDF).
	The advantage of these is that it is extremely space efficient and an elegant coding solution since I am using procedural generation, animation, and coding my own collision interactions that all will utilize these SDFs, Additionally the union, intersection, and disjoint set of these objects are extremely useful operations for modularity
	In summary, it doesn't make sense to go from a shape -> set of points -> shape and there shouldn't really be a performance hit for not baking the meshes since we can just reduce the precision of the rendering of the entities or the step size of the marching rays algorithm etc... So in short OOP enables Recursive Entity-Component and SDFs are an elegant data object for procedural generation, animation, and for collision detection

Data Structures:
	
Entities - Things that are spawned in the physcial world and visible
	Chunk (Physical) - The Actual physical game world itself, responsible for representing terrain as well as entities at their relative positions in that terrain. This class must be able to split and merge for dynamic performance reasons as well as save and load any edits to the default state 
	Vehicle (Both)/Building/Settlement/Biome- A container that holds itself as the root character, but enables the other charcters it holds to continue acting relatively to the vehicle... a building is the case where velocity is zero, and the settlement and the biome are simply more generalized buildings that likely span chunks but serve as a tool for higher level data abstraction and generation
	Characters (Abstract) / Structure- The entity class holding the model view abstract game data like the health and experience stats, available abilities etc...
		Race(Both)/Style:Humanoid, Mammal, Bird, Reptile, Crustacean, etc... Define generation algorithms (including animations) which determine the types of Parts and therefore joints and bones present can also include abstract data like racial abilities, actions, components, etc...
		Body (Physical) - The view-model physical game world object class it will have the list of spines and as well as store the root position rotation and velocity, the root acts as the origin for motion and the spines are chains of bones that connect to the root and are used for animations. Everything is collision detectable perhaps but first we will use the complex sphere of r = max spine length to determine if we use complex collision shapes, it will also be responsible for housing a lot of IK and FK helper methods?
			Spine(Physical) - The list of joints that are connected to the root, will have animaiton functionality
		Part (Abstract) - The individual physical objects that compose a body but as their abstract game data like their set of parameters to determine their bones and joints that they contribute towards the body
			Types:BodyParts,Organs,Armour,Tools,Resources, Food, Items,etc... Basically everything else in game
			Joint (Physical) - Represent the Transformations requried to combine separate objects, will have parameters restricting how the spine can alter these transformations as well as potentially override some functions itself or at least provide some poses
				Fixed - fixed in place for things like organs or armour Parts to slot into (armour will have a function that is the remainder after subtracting the body from it and will be a part just lacking a joint)
				Hinge - flex and extend in only one axis
				Saddle - bend in two axis
				Pivot - Bend in all axis
				Rotator - rotates (other joints can rotate by inheriting this class)
			Flesh (Physical) - Represent SDF's the actual mesh that gets moved around and merged with other meshes by the body, these can be animatible too but just wont be using kinematics, like for hand motion itll likely be simply alternating between different shapes for open and closed fist
				Use Library for primitive shapes and build my own library as I go: Branches, Leaves, Muscles, Hands, Head, Face
			Components(Abstract) - Standalone plug and chug game logic with local variables too, should respond  to signals and provide action api dependent on embedded or decoupled stats; complex component systems should be separate and send signals through the Part or the Character to avoid coupling and promote modularity
				Types:AIController,PlayerInput, MovementNavigator, Health, Material, Resistance...
				Actions(Abstract) - API in components that can be triggered, blocked or canceled with high enough priority, and do things for the player
		Abilities(Abstract) - A container for action functions, executing them in order but also providing a place to store modifiers like ability strength, efficiency, duration, base priority etc.. Default abilities are bound to inputs alternatively context abilities are simply the ability housing default modifiers with default filter of any of the actions of the selected Entity
			Filter(Abstract) - An executable piece of code that returns null or a specific action that is then run with parameters scaling with ability strength, efficiency, and for a certain duration and priority. (Make these swappable and editable???)
			Modifier(Abstract) - Something that alters the abilities base modifiers (stretch goal is to have this modify filters but how???)
