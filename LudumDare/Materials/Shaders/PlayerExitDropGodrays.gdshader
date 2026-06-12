shader_type spatial;
render_mode unshaded, blend_add, cull_disabled, depth_draw_never;

uniform sampler2D noise_tex;

uniform vec3 beam_color : source_color = vec3(1.0, 0.95, 0.8);

uniform float intensity = 1.0;
uniform float noise_scale = 4.0;
uniform float noise_speed = 0.02;

uniform float fade_start = 2.0;
uniform float fade_end = 0.5;

varying vec3 world_pos;

void vertex()
{
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment()
{
    vec2 uv = UV;

    vec2 noise_uv = vec2(uv.x * noise_scale, uv.y * 0.15 + TIME * noise_speed);

    float noise_value = texture(noise_tex, noise_uv).r;
    noise_value = pow(noise_value, 2.5);

    float length_fade = 1.0 - uv.y;
    float width_fade = 1.0 - abs(uv.x * 2.0 - 1.0);

    float camera_distance = distance(CAMERA_POSITION_WORLD, world_pos);

    float camera_fade = smoothstep(fade_end, fade_start, camera_distance);

    float alpha = noise_value * length_fade * width_fade * camera_fade * intensity;

    ALBEDO = beam_color;
    ALPHA = alpha * 0.35;
}