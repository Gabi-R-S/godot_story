using Godot;

public partial class CenteredSubViewportContainer : SubViewportContainer
{
    private Vector2 _baseSize; // Store the original size

    public override void _Ready()
    {
        // Store the original size before any scaling
        _baseSize = Size;

        CenterAndScaleContainer();
        GetViewport().SizeChanged += CenterAndScaleContainer;
    }

    public override void _ExitTree()
    {
        GetViewport().SizeChanged -= CenterAndScaleContainer;
    }

    private void CenterAndScaleContainer()
    {
        // Get the parent's size (usually the main viewport or a Control node)
        Vector2 parentSize;
        if (GetParent() is Control parentControl)
        {
            parentSize = parentControl.Size;
        }
        else
        {
            parentSize = GetViewport().GetVisibleRect().Size;
        }

        // Calculate the maximum integer scale that fits in both dimensions
        int maxScaleX = (int)(parentSize.X / _baseSize.X);
        int maxScaleY = (int)(parentSize.Y / _baseSize.Y);

        // Use the smaller of the two to maintain aspect ratio and fit entirely on screen
        int scaleValue = Mathf.Max(1, Mathf.Min(maxScaleX, maxScaleY));

        // Apply the scale (this magnifies the SubViewport content)
        Scale = Vector2.One * scaleValue;

        // Calculate the actual screen space taken up after scaling
        Vector2 scaledSize = _baseSize * scaleValue;

        // Center the scaled container
        Position = (parentSize - scaledSize) * 0.5f;
    }

}