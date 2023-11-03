#include "body.h"
#include <godot_cpp/core/class_db.hpp>

using namespace godot;

Body::Body(){

}

Body::~Body(){

}

void Body::_process(double delta){
    //moves the character by their velocity and then dapens the velocity by inertial dampener * delta
    velocity.move_toward(Vector3(0.0, 0.0, 0.0), delta * inertial_dampener);
}

// Base Movement Functions
// Wall Detection
bool Body::is_on_floor(){

}
bool Body::is_on_wall(){
    
}
bool Body::is_on_ceiling(){

}

// Modular functions for adding and removing parts
bool Body::equip_part_at(Node3D part, Node3D joint){

}
Node3D Body::remove_part_at(Node3D joint){
    for (Node3D part : parts){

    }
}