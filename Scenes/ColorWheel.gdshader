shader_type spatial;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 uvCentered = vec2(UV.x,UV.y) - vec2(0.5,0.5);
	float angle = acos(dot(UV-vec2(0.5,0.5),vec2(0,1)));
	if (uvCentered.x != 0.0f)
	{
		angle*= (uvCentered.x / -uvCentered.x);
	}
	angle += 360.0;
	angle = float(int(angle)%360) + (angle - floor(angle));
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
