[Title( "Post Process Playground" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class PostProcessPlayground : PostProcess, Component.ExecuteInEditor
{
	[Property] public Color Color { get; set; }

	IDisposable renderHook;

	protected override void OnEnabled()
	{
		renderHook = Camera.AddHookBeforeOverlay( "My Post Processing", 1000, RenderEffect );
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	RenderAttributes attributes = new RenderAttributes();

	public void RenderEffect( SceneCamera camera )
	{
		if ( !camera.EnablePostProcessing )
			return;

		// Pass the FrameBuffer to the shader
		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
		Graphics.GrabDepthTexture( "DepthBuffer", attributes );


		// Blit a quad across the entire screen with our custom shader
		Graphics.Blit( Material.Load( "materials/postprocessplayground.vmat" ), attributes );
	}
}
