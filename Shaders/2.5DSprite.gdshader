shader_type spatial;

render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx,unshaded;

uniform vec4 albedo : source_color = vec4(1.0,1.0,1.0,1.0);

uniform sampler2D sprite_sheet : source_color, filter_nearest;

/// the count of v_frames, used for the directions
uniform int directions_count = 1;
/// the count of h_frames, used for the animations
uniform int frames_count = 1;
/// the current h_frame
uniform int frame = 1;
/// the angle around vertical axis; the resulting direction looks to (cos y_angle, 0, -sin y_angle)
uniform float y_angle = 0.0;

/// this parameter below specifies which angle is the first frame pointed to.
/// example: if your character has the first frame turned toward north, the starting angle should be pi/2, if it looks to east it can be let to 0.0
uniform float starting_angle = 0.0;

/// this option specify if the direction changes along the vertical axis (default, false) or use the horizontal one instead (true)
uniform bool directions_on_horizontal_axis = false;
/// if you have drawn the sprite sheet so as to make the object rotate clockwise instead of counter-clockwise, set this parameter below true
uniform bool clockwise = false;

void vertex() {
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0],INV_VIEW_MATRIX[1],INV_VIEW_MATRIX[2],MODEL_MATRIX[3]);
}
void fragment() {
	//euler angle around the vertical axis
	float vert_axis_angle = atan(-VIEW_MATRIX[0][2]/VIEW_MATRIX[2][2]) + (VIEW_MATRIX[0][0] < 0.0?PI:0.0);
	//difference
	vert_axis_angle += y_angle - starting_angle;
	//pinning
	vert_axis_angle += PI / float(directions_count);
	float direction = floor(vert_axis_angle * float(directions_count) / TAU);
	if (clockwise) direction = 1.0 - direction;
	float uv_y = direction / float(directions_count);
	//texture UV
	vec2 texture_uv = directions_on_horizontal_axis?
	vec2(
		(UV.x / float(directions_count)) + uv_y,
		(UV.y + float(frame))/float(frames_count)
	):
	vec2(
		(UV.x + float(frame))/float(frames_count),
		(UV.y / float(directions_count)) + uv_y
	);
	vec4 albedo_tex = texture(sprite_sheet,texture_uv);
	//albedo
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	ALPHA = albedo.a * albedo_tex.a;
}