shader_type spatial;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	
	const vec2 redVec = vec2(0.0,-1.0);
	const vec2 greenVec = vec2(0.86602545,0.5);
	const vec2 blueVec = vec2(-0.86602545,0.5);
	
	vec2 uvCentered = vec2(UV.x,UV.y) - vec2(0.5,0.5);
	vec3 result;  
	result.x = dot(uvCentered, redVec);
	result.y = dot(uvCentered, blueVec);
	result.z = dot(uvCentered, greenVec);
	result += (vec3(1.0,1.0,1.0)* (1.0 - length(vec2(0.5,0.5)-UV)));
	
	ALBEDO = vec3(result.x,result.y,result.z);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
