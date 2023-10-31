v1.0.2
Endless RPG Design Document
Model-View Architecture
Advantages
The advantage of Model-View is the simplicity in managing networking code as only model data needs to be transmitted, as well as ensuring efficient performance. The key areas for these performance increases will be with the headless server performance, as well as the ease of implementing performance saving graphics settings without interfering with gameplay elements. Additionally, this view will make it simple to implement save/load systems since there won’t be a need to translate viewable objects into their data component (the model components will be essentially already translated)
Implementation
To achieve this structure, the abstracted game entities utilizing Entity Component System (ECS) Architecture will be pure data abstractions, and their rendered physical objects will be godot Node3D objects in the more traditional sense that will be instanced into the scene with a similar tree structure to the ECS system but more reliant on physical location hierarchy. This means that if a character is in a vehicle the Viewable Node3D Character class will be a child of the vehicle node, but the Modeled Node Character class will be its own root and a reference stored in the server chunk manager singleton (The exact implementation of chunk rendering is still TBD)
Entity Component System
Advantages
Very modular, an entity has data in components and a system operates on all entities based on the relevant components. Theoretically can do everything probably. Very nice if you want to be able to plug and play with different combinations of modifiers and components on objects, exactly what one would want in a sandbox rpg.
Disadvantages
A strict ECS system can be cumbersome. Want a component that is another component but slightly different and with a tweaked system? Well thats exactly what you have to do… write another system and add relevant components to everything that needs it. Want different systems for different entities? Add a component the gives a type variable to all entities and add that ass a parameter to both systems… Technically everything is feasible, but polymorphism and inheritance are super nice.
Modifications- Containers, Components, and Local Systems
How about making the entity the system? Make a general entity class and iterate on it with object orientated design to get the specific system implementations. A generic container will have components and other containers, how the root container inherits the child container components is specified in the container object implementations both for the root and for the child. Now each container will execute functions depending on its own components. 

One short coming however is that now these system container objects might have few components but a lot of systems constantly checking for components. So the solution is to have the containers split into two parts: the mandatory systems that are integral to their identity as the type of container they are, and then the functions that define any unique accommodations for any kind of component while leaving the rest of the components to contain their own system that is called from the container and acts locally on it. 

This modification is like tying the System to its relevent components such that the system only runs when the components are being rendered, but its a bit better because we are allowing the entity the freedom of managing the system’s behaviour depending on the type of entity. 

For example a player container with a fire component might have the result of taking health damage over time, but a tree container with the same component but also a flammable stat component would have the result of spreading the fire and spawning fire instances in it’s branches. 

This kind of polymorphic behavior of different containers with the same components isn’t impossible with the traditional strict ECS approach, but would be quite cumbersome.

As you’ve seen, now we have two types of components: Stats and Abilities. Stats are simply data and the traditional definition of a component, something ‘the container has _ ’, while abilities are the inherited systems that act locally from the containers and can be considered anything that is an action or is of the form ‘the container is being _ed’ or ‘the container is _ing’

To better understand abilities, we need to introduce another set of two components: poses and actions. Poses are viewable physical animations to play (however since the logic includes hitbox detection they are still important to the model, so anything with hitboxes and and poses can not be occluded). While actions are calls to container’s system API functions like players’ move_dir(direction) function being called from a walk ability. Except poses and actions aren’t real components since they are constant for each type of container.

(Note that for readability, it makes sense to type actions and sub section them off into system components, but these components are mandatory to be instanced with their respective containers)

So our abilities will simply be a sequence that when triggered will sequentially go through a set of poses performing different actions. The specific pose and action will be inferred by the container type local to our ability component, additionally, the speed of this sequence and parameters going into these actions will be affected by scalings (hard coded weights to different stat values that the local container has or the root container has)

Now, the only issue is that sure there is a modular character and the stats are quite easily adjustable, but the abilities seem quite static. To add modular abilities we need a final component for components called a modifier. This will act on the ability itself whenever the ability is called. This could include inserting poses and actions, removing them from the set (temporarily of course) adjusting the weights, etc…. This enables the use of general synergies and buffs for different classes of abilities and such as well as the ability to create unique abilities by stacking modifiers.

Class Design (Purpose):
Containers: These contain a list of type tags so they can be sorted
	Systems-A set of always active systems that operate without being triggered by abilities, like on spawn, on death, on collision, or more along the lines of PlayerInput and NPCControl or EnemyController etc… these are written into the container class and are also responsible for spawning the default actions.
Abilities-A sequence of Actions and Poses depending on weighted stats that when triggered triggers the set of actions and poses, or if it has any modifiers triggers on the result of the modifier. The sequence will be of a list of (action, priority, duration) where a wait action of the default container class can work as a warmup and cooldown.
Recipes-A formula for Input container objects into output container objects dependent on each container’s components, very similar to abilities except the sequence isn’t about timing but about resource consumption and theres no need for priority and modifiers can be applied by parent of the recipe’s container not just as a child of the recipe 
Components: These should also contain a list of type tags so they can be sorted…
	Stats-Pure Data, can be static or dynamic
	Modifiers(Ability or Recipe)-Basically another ability especially since it returns an ability, that is simply supposed to act on an existing ability, so calling it unattached to an ability will likely return an empty set.
	Actions-An API that when triggered does something to alter the game state depending on the present stats but and is separated into System Components, these components only act when triggered and also are constant among the same containers, (it’s also likely that additional actions can be added to containers as a result of abilities that apply buffs, for example receiving the onfire buff will provide the burning action):
		Poses-A specific type of action for altering relative 3D positions of child or current containers
		Movement-Actions that change the physical location of the root container
		Vision-Actions that change what is visible or alter the camera
		Utility-Actions that are related to equipping/uneqipping stuff etc…
		Persistent-Actions that are repeated on a regular interval
		Combat-Actions that are related to calculating incoming and outgoing damage based on stats… calc_attribute(strength) for example calc_resistance(piercing) for example. 
Specific Class Inheritance:
Sortable: Has a list of keywords that can be filtered for, also name and description and alphabetical comparator
Container: can contain any stat but only some modifiers or actions depending….
Entity: something existing in the chunk with an active collision hitbox
	Structure: something that doesn’t have body parts and equippables but just parts? Idk why i would need this over just continuing to use characters….
	Character: a pointer to a root body part, some default actions, and maybe even some stats
		PlayerCharacter: includes a player input management system
Part: something physical with a hitbox and mesh but collision isn’t enabled only hit detection (parts phase through other parts, but entity’s cannot)
Equippable: Parts that can be slotted into sockets that arent bodyparts (leafs)
	Organ: Something that is internal and gives actions and applies modifiers probably also probably contributes attributes
	Armour: Something that is either painted onto the part of simply worn ontop of the part, it has a different set of base stats and can have actions and give abilities
Tool: Same with armour but is just different

To be honest there is no reason as of now to create a new class for Organs,Armour, or Tools except to make them mutually exclusive which might be sufficient. I guess i could have them generate default stats like organs will have blood consumption and armour will have a resistance profile and tools will have material and durability…

BodyPart: A linked graph connected to other body parts by sockets and has stats and actions justlike a character, basically part is the node class, character is the pointer to the root, and Bodypart is a specialized node while Equippable is a generic leaf class
Ability: only really here so that it can be a container for a bunch of modifiers
Recipe: same as above ^, should hold a list of order and priority of modifiers
Component: guaranteed to be leaves of the tree, can have systems that operate from the leaf position but besides persistent actions must be triggered
Stat: pure data and the only thing that is a legit modular ECS universal component…
	Attribute: static changes infrequently, describes default/optimal condition
	Value: dynamic, constantly changing, triggers actions on certain values, describes the current condition
Socket: A relative location and orientation to hold an equippable with a filter function, technically isnt a leaf in view mode but is one in model vew
RecipeModifier: takes in a recipe and returns a modified recipe (does nothing on a non recipe)
AbilityModifier: takes in an ability and returns a modified ability (does nothing on a non ability)
Action: Some are default to each container type, others can be added (however, not all are meaningful on all types of containers so this needs to be implemented safely), all can receive parameters and be triggered
	Movement: move_towards, set_velocity,... 
	Vision: look_at, set_camera
	Combat: calculate_damage…
	Utility: kill, spawn, unequip/equip
	Persistent: breath, must be turned on or off… 
	

V1.0.0
This project is primarily to create a video game that appeals to myself. 

The genres I hope to touch on and their inspirations are as follows:
	Rouge Like: Ability/Build modification and Loot RNG dependent, Minecraft Psi mod 'spell modification depth' but with Noita alchemy like complexity, enabling each player and each character to develop their own unique stlye of gameplay without the domince of 'optimal builds and strategies'
	FPS: Fast and Smooth Movement, Sliding, Mantling, Wall running, Grappling hooks, etc... Warframe is a good example, people play games to have fun, fast and smooth movement is fun, walking and wasting time isn't
	Souls-Like: Skill based combat, telegraphed heavy attacks requiring dodge/parries, directional swings and spacing, sponge like bosses and simply seeing damage numbers go up is the bare minimum for increased difficulty in combat. By building the system with enough complexity at its foundation, it enables skill based increase in difficulty as opposed to purely stat scaling.
	Open World: Infinite terrain generation with dungeones, biomes (resources, wildlife, enemies), simulated economy, even politics eventually? More importanly a diverse array of elemental and mundane interactionss: Fire, Liquids, Gasses, Gravity, Force(Wind), Time Dilations etc... Biomes will be themed around these and flora and fauna will have abilities with these modifications. The ability to adventure and explore in a virtual world freely is simply unparalleled. Theres nothing revolutionary about implementing procedural world generation as opposed to a handcrafted world so why not?
	Sandbox: Persistent progression, player will have a mobile base (air ship, normal ship, pocket dimension etc...), can build a place to live and store momentos from adventures, but also crafting progression where engaging with increased complexity yields increased efficiency and benefits, using factorio like assembly lines and automations to create gear and equipment. But also the player character themselves are a canvas to be edited. Rimworld like body modifications to affect stats and to provide different alchemical substances to be utilized in the modular abilities. Being able to explore an infinite world is pointless if youre restricted by inventory space or the inability to bring your creations with you and also reduced if youre unable to interact and contribute to it. I aim to solve this by allowing the player to simply bring their 'base' along with them as they travel and making it a fundamental to why they may wish to extract resources from the environment or to sell of their produced goods into the economy. 
	MMO: Very stretch goal is to make this work with multiplayer.... maybe have singleplayer instances be islands that can be merged and have hub worlds and contested resource worlds as well as all the other good features and competitve pvp tournaments...
	
All of these goals are pretty ambitious and see a lot of feature creep. The current objectives I have set for myself:
	Modular Character System:
		Attacks:
			Stances- i.e. Torso has various slashing and thrusting stances that manipulate the arm, the arm has stances that manipulate the hand... and then the sword will detect collisions and apply damage, if it has magical attributes it might also trigger them like a force blade pushing the hitbox out from the sword..
			Styles- stlyes are a sequence of stances and other effects chained together with other data like speeds and strength modifiers triggered by a player attack action. Mashing the attack button will simply go through the sequence of attacks in order while directional inputs can select certain attacks specifcally enabling both a simple and advanced combat mode
			Hitboxes/Damage - like rimworld each limb will have health and store a log of the damage and damage type recieved so it can be healed with different healing abilities (scars are different from burns and cuts etc...), taking damage may reduce a limbs efficiency so they contribute less stats to the character overall, when the limb or organ is destroyed it can be dropped and then later fixed / requipped or processed for other materials etc...
		Equip/Unequip- I want to utilize the Entity Component System paradigm to create an extremely modular character system but my object orientated programming background is making this difficult. Each limb should be able to work with any combination of organs and equipment and any character should function with any combination of limbs, I am considering having containers like characters, bodyparts, organs, equipment, armour, weapons, etc... be objects and their contents be the components. This way a health component on a piece of equipment vs bodypart vs character will function differently since the system will be unique to the class
			Loot/RNG: There should be leveling and skill progression so that using limbs and stuff enables the increase in their stats (EVs), but they should also have base stats (IVs) that are randomized and genetic. This enables the casual player to simply level, but the dedicated player to grind for optimal organs
		Movement - I'm not using root motion (not yet at least), so I want to sepearate the animations from logic, the goal is to make it not too apparent this is happening, but janky is fine for now. Having spider legs may grant the ability to climb on walls and stuff like that, so the movement code needs to be very modular with the ability to add and remove various abilities given by the limbs (person without a head cant look around, no legs can jump or walk...) The issue becomes this would be very unfun if you loose a limb.... maybe a soul third person default camera? On death a ghost mode until respawn?			
		Abilities: So far there seems to be 2 different types of component classes: Stats and Abilities
			Stats: This is pure numerical data, bigger stats are better, make you hit harder and faster, take more damage etc...
			Abilities: These are functions that the player can call that do things. None of them are default and are added by organs and limbs that provide them. These abilities are often modified by their container's stats, their parent's stats, or the character's stats. And they often cast a signal down the entire character signaling animations to be played for any node that knows how to play it
				Movement Abilities: turn towards, look up/down, walk, run, jump, slide, crouch, fly, teleport
				Combat Abilities: L/R/U/D slash, thrust, kick
				Utilitiy Abilities: Chat, Hear, Pick-up, Put-Down, Craft....
	Crafting: Currently there is no inventory system so this gives a good reason to implement the crafting systems
		Resource Gathering - Enemies will drop their organs, limbs, and gear with different rates depending on death type (slashing increases limb drops) and damage (preservation), while flora like trees will drop wood (coded similarily)
		Transportation:
			Soul Space - To increase QOL there will be a simple magnet effect that (add a filter function) grabs resources and stores them into a sortable list of items (add icons?), but in future iterations this should be progression gated and one should rely on physical manipulation or mundane inventories and such
			Physical Manipulation:
				Picking things up- reach and hold them in ones hand so they travel with or to simply store in an equipped inventory like a back pack
				Levitation- an effect to hover objects as they travel with the player, push pull rotate, and then set them down
				Throwing- tossing an equipped object (should deal damage and have a trajectory dependent on mass etc...)
		Crafting: The stats of the material should contribute to the process in terms of both final product stats, but also process speed and efficiency (if using fuel or something)
			Stations: Stations take in inputs and produce outputs based on their list of recipes. Stations have generic classes that define the set of base recipes but variations of these stations like higher tiers may have tweaked recipes that change crafting time and resource ratios...
			Recipes: The formula of inputs to outputs as well as describing how the input material stats translate to the output material stats
			Buidlings: are the physical structures with the abstract stations housed within, buildings can have more than one station and the inputs and outputs can be physicalized areas in the world with the recipe selection taking place in a pop up gui tree list (maybe sorted by valid recipes?)
			Soul Space Crafting: Some stations for basic crafting should be able to be done in the soul space using that as input and output


V1.0.1
So I have the following thoughts on the structure of the game. I think my current code needs another rewrite lol

Endless RPG Design Document
Model-View Architecture
Advantages
The advantage of Model-View is the simplicity in managing networking code as only model data needs to be transmitted, as well as ensuring efficient performance. The key areas for these performance increases will be with the headless server performance, as well as the ease of implementing performance saving graphics settings without interfering with gameplay elements. Additionally, this view will make it simple to implement save/load systems since there won’t be a need to translate viewable objects into their data component (the model components will be essentially already translated)
Implementation
To achieve this structure, the abstracted game entities utilizing Entity Component System (ECS) Architecture will be pure data abstractions, and their rendered physical objects will be godot Node3D objects in the more traditional sense that will be instanced into the scene with a similar tree structure to the ECS system but more reliant on physical location hierarchy. This means that if a character is in a vehicle the Viewable Node3D Character class will be a child of the vehicle node, but the Modeled Node Character class will be its own root and a reference stored in the server chunk manager singleton (The exact implementation of chunk rendering is still TBD)
Entity Component System
Advantages
Very modular, an entity has data in components and a system operates on all entities based on the relevant components. Theoretically can do everything probably. Very nice if you want to be able to plug and play with different combinations of modifiers and components on objects, exactly what one would want in a sandbox rpg.
Disadvantages
A strict ECS system can be cumbersome. Want a component that is another component but slightly different and with a tweaked system? Well thats exactly what you have to do… write another system and add relevant components to everything that needs it. Want different systems for different entities? Add a component the gives a type variable to all entities and add that ass a parameter to both systems… Technically everything is feasible, but polymorphism and inheritance are super nice.
Modifications- Containers, Components, and Local Systems
How about making the entity the system? Make a general entity class and iterate on it with object orientated design to get the specific system implementations. A generic container will have components and other containers, how the root container inherits the child container components is specified in the container object implementations both for the root and for the child. Now each container will execute functions depending on its own components. 

One short coming however is that now these system container objects might have few components but a lot of systems constantly checking for components. So the solution is to have the containers split into two parts: the mandatory systems that are integral to their identity as the type of container they are, and then the functions that define any unique accommodations for any kind of component while leaving the rest of the components to contain their own system that is called from the container and acts locally on it. 

This modification is like tying the System to its relevent components such that the system only runs when the components are being rendered, but its a bit better because we are allowing the entity the freedom of managing the system’s behaviour depending on the type of entity. 

For example a player container with a fire component might have the result of taking health damage over time, but a tree container with the same component but also a flammable stat component would have the result of spreading the fire and spawning fire instances in it’s branches. 

This kind of polymorphic behavior of different containers with the same components isn’t impossible with the traditional strict ECS approach, but would be quite cumbersome.

As you’ve seen, now we have two types of components: Stats and Abilities. Stats are simply data and the traditional definition of a component, something ‘the container has _ ’, while abilities are the inherited systems that act locally from the containers and can be considered anything that is an action or is of the form ‘the container is being _ed’ or ‘the container is _ing’

To better understand abilities, we need to introduce another set of two components: poses and actions. Poses are viewable physical animations to play (however since the logic includes hitbox detection they are still important to the model, so anything with hitboxes and and poses can not be occluded). While actions are calls to container’s system API functions like players’ move_dir(direction) function being called from a walk ability. Except poses and actions aren’t real components since they are constant for each type of container.

(Note that for readability, it makes sense to type actions and sub section them off into system components, but these components are mandatory to be instanced with their respective containers)

So our abilities will simply be a sequence that when triggered will sequentially go through a set of poses performing different actions. The specific pose and action will be inferred by the container type local to our ability component, additionally, the speed of this sequence and parameters going into these actions will be affected by scalings (hard coded weights to different stat values that the local container has or the root container has)

Now, the only issue is that sure there is a modular character and the stats are quite easily adjustable, but the abilities seem quite static. To add modular abilities we need a final component for components called a modifier. This will act on the ability itself whenever the ability is called. This could include inserting poses and actions, removing them from the set (temporarily of course) adjusting the weights, etc…. This enables the use of general synergies and buffs for different classes of abilities and such as well as the ability to create unique abilities by stacking modifiers.

Class Design:
Containers: These contain a list of type tags so they can be sorted
	Systems-A set of always active systems that operate without being triggered by abilities, like on spawn, on death, on collision, or more along the lines of PlayerInput and NPCControl or EnemyController etc…
	Actions-An API that when triggered does something to alter the game state depending on the present stats but is separated into System Components for Readability, these components only act when triggered but also contain a list of type tags:
		Poses-A specific type of action for altering relative 3D positions of child or current containers
		Movement-Actions that change the physical location of the root container
		Vision-Actions that change what is visible or alter the camera
		Utility-Actions that are related to equipping/uneqipping stuff etc…
		Combat-Actions that are related to calculating incoming and outgoing damage based on stats… get_modifier(strength) for example get_resistance(piercing) for example. This goes here instead of in the abilities that will call this so that if I want ogres for example to just be stronger even when using the same abilities I can
Abilities-A sequence of Actions and Poses depending on weighted stats that when triggered triggers the set of actions and poses, or if it has any modifiers triggers on the result of the modifier. The sequence will be of a list of (action, priority, duration) where a wait action of the default container class can work as a warmup and cooldown.
Components: These should also contain a list of type tags so they can be sorted…
	Stats-Pure Data, can be static or dynamic
	Modifiers-Basically another ability especially since it returns an ability, that is simply supposed to act on an existing ability, so calling it unattached to an ability will likely return an empty set. 
