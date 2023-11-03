#ifndef BODY_H
#define BODY_H

#include <godot_cpp/classes/node3d.hpp>

using namespace godot;

/*
The body class acts as a character body 3d node except it's collision detection is from recieved signals
*/
class Body : public Node3D{

    GDCLASS(Body, Node3D);

    private:
        

    protected:
        static void _bind_methods(){};

    public:
        Body();
        ~Body();

        void _process(double delta);
        //Velocity and Acceleration Methods for when physics processes like collisions occour
        //Internal variables for moving the character, Node handles position and orientation, these handle velocity and acceleration
        Vector3 velocity;
        Vector3 angular_velocity;
        //used essentially as a coefficient of friction, vel = vel * 1 / (1 + delta*inertial_dampener)
        float inertial_dampener;
        
        //This vector is the relative position the body will attempt to move itself towards regardless of physics processes
       //Vel and acceleration are for gravity this is for voluntary movement
        Vector3 target_position;
        float speed;

        //Base Movement Functions
        //Wall Detection
        bool is_on_floor();
        bool is_on_wall();
        bool is_on_ceiling();
        //these are the animation abilities to be called when given an input vector
        //they will give the 
        std::list<Node3D> locomotions;

        //Modular functions for adding and removing parts
        bool equip_part_at(Node3D part, Node3D joint);
        Node3D remove_part_at(Node3D joint);

        //List of body part objects
        std::list<Node3D> parts;

        //List of Joint objects, these are collision shapes and have capsules spanning between them
        std::list<Node3D> joints;

};

#endif