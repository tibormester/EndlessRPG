#pragma once

#include <godot_cpp/classes/node3d.hpp>

using namespace godot;

class Test : public Node3D{

    GDCLASS(Test, Node3D);

    protected:
        static void _bind_methods(){};

};