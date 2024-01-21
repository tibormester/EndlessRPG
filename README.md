#Endless RPG

**Summary**

	This project aims to create a *sandbox rpg* experience, ideally enabling players to become immersed in a fantasy world with the freedom to interact with it however they choose. Set in a chaotic fantasy world, it will incorporate not only swords and spells but encounters ranging from sky pirates aboard flying ships with runic cannons to steampunk gnomic golems guarding forgotten underground cities to nomadic druid settlements in harmony with their environment. The aim is to build up this world by iteratively implementing each niche setting of the genre with not prebuilt assets but through procedural generation. Along with unique mechanics for each setting's own flavor of combat, crafting, and civilization building, there will be new algorithms/traits for the existing world generation and mechanics to pick from, resulting in a game where no two save files will be the same.
 
-------

#Progress

**Models**
	For this project, I have avoided using assets. In hindsight, this has been quite the delay as I have had to learn to create these assets through script, which is a lot harder than anticipated. However, it has been quite rewarding and has helped me better understand *Unity* as an engine. The idea of avoiding assets was primarily to make things easier for the procedural generation of the characters. The obvious approach in hindsight is to have assets for each body part and scale them a bit as well as pick randomly from a pool of them for variation. However, I decided to simply sketch them out as a bunch of quadrilaterals defining their dimensions at varying heights and quickly creating meshes from that data. This method gives me more control over the mesh data itself, but that level of control isn't necessary. Especially since a lot of runtime tweaks to the mesh will be done with shaders regardless and having each body part as a separate mesh instead of a skinned mesh might have been simpler for things like dismemberment... So all that said, I will likely maintain this workflow of sketching out models as basic geometric shapes through code and when this project reaches the polishing stage I will aim to hire an artist to create assets and refactor this code.
 	Part of creating the mesh was learning that *Unity* doesn't seem to want people doing too much animating through script. Sure creating the mesh itself is fine and documented enough, but both the legacy and new animation systems are a struggle to get working without prebuilt animation assets. And even then they seem to lack libraries for generating more complex animation curves on the fly. As a result, I am setting out to simply ignore the animation systems and write my own self-contained animation components for objects that need it. Part of this is using IK specifically FABRIK and Trig for 2-bone IK. This will make it easier to incorporate more dynamic animations since it should come more naturally; however, I have started to notice it also leads to more convoluted systems without the easy-to-use state machine GUI. I hope to resolve this issue soon (As well as implement rotations and constraints to my Fabrik algorithm).
  
**Character Controller**
	So for a character, I have a rigid body with rotation locked and no gravity that is pushed around by forces. Although there is a box collider as a hitbox for the character, it remains floating above a pair of ray casts, one straight down, and another in the direction of motion (clamped to 45 degrees). These ray casts look for the ground and when found linearly phase out gravity while introducing a spring force with velocity dampening that aims to keep the boxcollider hoovering. The result is that going over minor bumps and debris is done seamlessly without constant raytracing for collisions to step over. The downside for some applications is that the character controller might feel slightly floaty and may bounce, but by tweaking dampening values and spring strength I was able to achieve a result that feels decent. Additionally, the responsiveness of the character controller was later fixed after I implemented forces that dampen velocity in non-movement input directions.
 	All in all, there are a few of the very basic movement features: jumping/doubling jumping, turning the body towards movement while turning the head towards the camera, rotating to be normal to surfaces, and instant changes in directions and drag. This all creates a physics-based controller that feels pretty responsive, which is ideal. Implementing these features sounds simple, and it is; however, after starting this project and going through this phase a handful of times, no two attempts have been the same. Fine-tuning and tweaking decisions made on the character controller is a complex topic that will need more polish and is currently beyond my limited experience.

-------

**Current Objectives**
* **Combat** 
* Sword-Swinging / thrusting actions, mouse screen position as aiming
* Body part health, dismemberment, active ragdoll
* Enemy AI, approach then circle, then attack
* **Environment**
* Generate terrain
* Generate plants (trees, bushes, rocks etc...)
* **Crafting**
* Have recipes for cutting down a tree and utilizing the logs for a palisade
* Have recipes for cooking monster meat into a consumable for restoring lost limbs
  
-------

#Vision

**Combat**
	Physics-based combat is by far the most immersive and simply cool. However, it can be frustrating, clunky, and hard to learn. Titles like *Mordhau* and *Chivalry* focus on realism, but in this fantasy setting without competitive multiplayer, I want to have a more arcade-like feeling similar to *Warframe*. To accomplish this, players will have access to features like wall running, grappling hooks, short-range dashes/blinks, etc... movement should be King. Additionally, the physics-based combat will enable the introduction of the relatively uncommon concept of physical health bars. Similar to *Rimworld* or *Kenshi*, an avatar's body parts will contribute stats and have hitpoints, so when going into a fight there's no guaranteed number of hitpoints needed to kill and certain lost hitpoints will be more debilitating than others. By adding onto combat with damage types, resistances, and effects with each iteration of a new fantasy flavor, the possibilities for this system as a mechanic for generating more immersive encounters are endless. (I hope to avoid pigeonholing the player into a single progression path or a handful of viable builds. Targeting weak spots should enable those with the tenacity to overcome every encounter, while the variety of combat mechanics should make every build equally overpowered)

**Crafting**
	A sandbox game doesn't exist without some form of crafting. Nearly every MMO is a combination of Combat, RPG, and Economy. But in a single-player game, aiming for an entire Economy is a bit ambitious so I will settle for simply crafting. This aspect of the game design may be a bit selfish, but I have always been driven to games where the Economy is Tall and Wide. I want to be able to have ownership of every part of the process of harvesting resources, creating the goods, to shipping and selling; while at the same time having enough complexity in the systems that there are plenty of viable points of entry into the economy. I know this is an ambitious goal which is why I hope that with each iteration of a new fantasy flavor, not only will I grow the breadth of the crafting system, but also the depth. For example, the flying pirates will come with the need for hiring guards for shipping lanes as well as the ability to craft one's own flying ships.

 	
