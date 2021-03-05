using System;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderToTexture : MonoBehaviour
{
    public RenderTexture TargetTexture;
    public SpriteRenderer Sprite;
    public SpriteMask Mask;

    private const int pixelsPerUnit = 100;

    private MaterialPropertyBlock propertyBlock;
    private CommandBuffer commandBuffer;
    private Mesh spriteMesh, maskMesh;

    private void Awake ()
    {
        propertyBlock = new MaterialPropertyBlock();
        commandBuffer = new CommandBuffer();
        spriteMesh = BuildSpriteMesh(Sprite.sprite);
        maskMesh = BuildSpriteMesh(Mask.sprite);
    }

    private static Mesh BuildSpriteMesh (Sprite sprite)
    {
        var mesh = new Mesh();
        mesh.hideFlags = HideFlags.HideAndDontSave;
        mesh.name = $"{sprite.name} Sprite Mesh";
        mesh.vertices = Array.ConvertAll(sprite.vertices, i => new Vector3(i.x, i.y));
        mesh.uv = sprite.uv;
        mesh.triangles = Array.ConvertAll(sprite.triangles, i => (int)i);
        return mesh;
    }

    private void Update ()
    {
        PrepareCommandBuffer();
        DrawMesh(maskMesh, Mask);
        DrawMesh(spriteMesh, Sprite);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }

    private void PrepareCommandBuffer ()
    {
        commandBuffer.Clear();
        commandBuffer.SetRenderTarget(TargetTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        commandBuffer.SetProjectionMatrix(BuildProjectionMatrix());
    }

    private Matrix4x4 BuildProjectionMatrix ()
    {
        var drawDimensions = new Vector3(TargetTexture.width, TargetTexture.height);
        var drawPosition = transform.position * pixelsPerUnit;
        var orthoMin = -drawDimensions / 2f + drawPosition;
        var orthoMax = drawDimensions / 2f + drawPosition;
        return Matrix4x4.Ortho(orthoMin.x, orthoMax.x, orthoMin.y, orthoMax.y, float.MinValue, float.MaxValue);
    }

    private void DrawMesh (Mesh mesh, Renderer renderer)
    {
        var drawPosition = renderer.transform.position * pixelsPerUnit;
        var drawRotation = renderer.transform.rotation;
        var drawScale = renderer.transform.lossyScale * pixelsPerUnit;
        var drawTransform = Matrix4x4.TRS(drawPosition, drawRotation, drawScale);
        renderer.GetPropertyBlock(propertyBlock);
        commandBuffer.DrawMesh(mesh, drawTransform, renderer.material, 0, -1, propertyBlock);
    }
}
