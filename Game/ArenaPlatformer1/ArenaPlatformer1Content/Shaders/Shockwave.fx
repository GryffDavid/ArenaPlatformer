float4x4 Projection;

void SpriteVertexShader(inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, Projection);
}

uniform extern texture ScreenTexture;

sampler ScreenS = sampler_state
{
	Texture = <ScreenTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = mirror;
	AddressV = mirror;
};

const float2 Resolution = float2(1920, 1080);
float2 SourcePos;
float4 WaveParams;
float CurrentTime;
float2 CenterCoords;

float4 PixelShaderFunction(float2 fragCoord: TEXCOORD0) : COLOR
{
	float2 HalfPixel = float2(0.5 / Resolution.x, 0.5 / Resolution.y);

	float3 waveParams = float3(10.0, 0.8, 6.1);
	float2 tmp = CenterCoords;
	float2 uv = fragCoord.xy;
	float2 texCoord = uv;

	float dist = distance(uv, tmp);

	float4 original = tex2D(ScreenS, texCoord + HalfPixel);

	if ((dist <= ((CurrentTime)+waveParams.z)) && 
		(dist >= ((CurrentTime)-waveParams.z)))
	{
		float diff = (dist - (CurrentTime));
		float powDiff = (1.0 - pow(abs(diff*waveParams.x), waveParams.y));

		float diffTime = diff * powDiff;
		float2 diffUV = normalize(uv - tmp);
		texCoord = uv + ((diffUV * diffTime) / (CurrentTime * 3000 * dist));
		original = tex2D(ScreenS, texCoord + HalfPixel);
		original += (original * powDiff) / (CurrentTime * 1000 * dist);
	}

	return original;
}

technique
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}