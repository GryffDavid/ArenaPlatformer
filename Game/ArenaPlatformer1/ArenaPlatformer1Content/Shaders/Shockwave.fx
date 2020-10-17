float4x4 Projection;

void SpriteVertexShader(inout float2 texCoord : TEXCOORD0, inout float4 position : SV_Position)
{
	position = mul(position, Projection);
}

uniform extern texture ScreenTexture;

sampler ScreenSampler = sampler_state
{
	Texture = <ScreenTexture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = mirror;
	AddressV = mirror;
};

const float2 Resolution = float2(1920, 1080);
float4 WaveParams;
float CurrentTime;
float2 CenterCoords;

float4 PixelShaderFunction(float2 fragCoord: TEXCOORD0) : COLOR
{
	float3 WaveParams = float3(10.0, 0.5, 0.1);

	float ratio = Resolution.x / Resolution.y;

	//vec2 WaveCentre = vec2(0.5, 0.5)* vec2(ratio, 1.0);   
	float2 WaveCentre = CenterCoords * float2(ratio, 1.0);
	//float2 texCoord = fragCoord.xy / Resolution.xy;
	float Dist = distance(fragCoord * float2(ratio, 1.0), WaveCentre);
	
	float4 Color = tex2D(ScreenSampler, fragCoord);

	//Only distort the pixels within the parameter distance from the centre
	if ((Dist <= ((CurrentTime) + (WaveParams.z))) &&
		(Dist >= ((CurrentTime) - (WaveParams.z))))
	{
		//The pixel offset distance based on the input parameters
		float Diff = (Dist - CurrentTime);
		float ScaleDiff = (1.0 - pow(abs(Diff * WaveParams.x), WaveParams.y));
		float DiffTime = (Diff  * ScaleDiff);

		//The direction of the distortion
		float2 DiffTexCoord = normalize(fragCoord * float2(ratio, 1.0) - WaveCentre);

		//Perform the distortion and reduce the effect over time
		fragCoord += ((DiffTexCoord * DiffTime) / (CurrentTime * Dist * 4.0));
		Color = tex2D(ScreenSampler, fragCoord);

		//Blow out the color and reduce the effect over time
		Color += (Color * ScaleDiff) / ((CurrentTime + 2.25) * Dist * 4.0);
	}

	return Color;
}

technique
{
	pass P0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}