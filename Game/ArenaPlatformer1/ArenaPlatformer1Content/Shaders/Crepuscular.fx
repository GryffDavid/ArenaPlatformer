float4x4 Projection;
void SpriteVertexShader(inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, Projection);
}

sampler2D UserMapSampler : register(s0);

uniform extern texture ColorMap;  
sampler ColorSampler = sampler_state
{
        Texture = <ColorMap>;     
};

uniform extern texture OccMap;  
sampler OccMapSampler = sampler_state
{
        Texture = <OccMap>;     
};

Texture DepthMap;
sampler DepthMapSampler = sampler_state 
{
	texture = <DepthMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

float2 LightPosition = float2(0.51, 0.5);
float decay= 0.9999;
float exposure=0.13;
float density=0.826;
float weight=0.358767;
const int NUM_SAMPLES = 48;

float2 offset = float2(0.5/1920.0, 0.5/1080.0);

//THIS HAS BEEN COPY-PASTED AND THEN MODIFIED FROM VERSION 2.
//NOT THE SAME AS VERSION 18 OR 17
float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float2 tc = texCoord.xy;
	float2 deltaTexCoord = (tc - LightPosition.xy);
	deltaTexCoord *= 1.0 / float(NUM_SAMPLES) * density;
	float illuminationDecay = 1.0;

	float4 color = tex2D(ColorSampler, tc.xy);// * 0.2;
		
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		tc -= deltaTexCoord;	
		float4 sample;
		float4 thing;

		//sample = float4(1,1,1,1) * tex2D(UserMapSampler, tc) * 0.2;
		//sample = float4(0,0,0,1) * tex2D(UserMapSampler, tc) * 0.2;

		//if (tex2D(DepthMapSampler, texCoord).r > 0.5f)
		//{
		//	sample = tex2D(OccMapSampler, tc) * tex2D(UserMapSampler, tc) * 0.2;
		//}
		//else
		//{
		//	sample = float4(1,1,1,1) * tex2D(UserMapSampler, tc) * 0.2;
		//}

		//sample = tex2D(OccMapSampler, tc) * tex2D(UserMapSampler, tc) * 0.2;

		if (tex2D(DepthMapSampler, tc).r > 0.5)
		{
			thing = tex2D(OccMapSampler, tc);
		}
		else
		{
			thing = float4(1,1,1,1);
		}

		sample = thing * tex2D(UserMapSampler, tc) * 0.2;

		sample *= illuminationDecay * weight;
		color += sample;//  * float4(1,0,0,1); Multiply by colour here to change light colour
		illuminationDecay *= decay;
	}

	float4 realColor = tex2D(ColorSampler, texCoord.xy + offset);
	
	return ((float4((float3(color.r, color.g, color.b) * exposure), 1)));// + (realColor * (1.1)));	
	
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
