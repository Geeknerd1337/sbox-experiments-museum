
MODES
{
    Default();
    VrForward();
}


FEATURES
{
}

COMMON
{
    #include "postprocess/shared.hlsl"
};


struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
	
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        
        o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

PS
{
    #include "postprocess/common.hlsl"
    #include "postprocess/functions.hlsl"
    #include "procedural.hlsl"

    #include "common/classes/Depth.hlsl"

    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

	SamplerState MySampler < Filter( Point ); >;
    SamplerState MySamplerDepth < Filter( Point ); >;

    // Passed framebuffer if you want to sample it
    Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); SrgbRead( true ); >;
    Texture2D g_tDepthBuffer < Attribute( "DepthBuffer" ); SrgbRead( false ); >;
    //CreateTexture2D( g_tDepthBuffer ) < Attribute( "DepthBuffer" ); 	SrgbRead( false ); Filter( MIN_MAG_MIP_POINT ); AddressU( CLAMP ); AddressV( BORDER ); >;
    float3 vMyColor < Attribute("mycolor"); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {

        float2 vScreenUv = CalculateViewportUv( i.vPositionSs.xy );
		float4 color = g_tColorBuffer.Sample( MySampler, i.vTexCoord );
        float4 depth =g_tDepthBuffer.Sample( MySamplerDepth, vScreenUv );
		float3 depth2 = Depth::GetWorldPosition(i.vTexCoord * g_vRenderTargetSize);
        float distance = length( float3(0.0,0.0, 0.0) - depth2 );
        return float4( 0.0, color.g, step( distance, 50.0f ), 1.0f );
    }
}