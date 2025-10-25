using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace Klak.TestTools
{

    public class MatPipe : MonoBehaviour
    {

        public Texture Texture => OutputBuffer;

        // Output options
        [SerializeField] RenderTexture _outputTexture = null;
        [SerializeField] Vector2Int _outputResolution = new Vector2Int(1920, 1080);
        [SerializeField] Material _materialName;

        UnityWebRequest _webTexture;
        WebCamTexture _webcam;
        Material _material;
        RenderTexture _buffer;

        RenderTexture OutputBuffer
          => _outputTexture != null ? _outputTexture : _buffer;

        // Blit a texture into the output buffer with aspect ratio compensation.
        void Blit(Texture source, bool vflip = false)
        {
            if (source == null) return;

            var aspect1 = (float)source.width / source.height;
            var aspect2 = (float)OutputBuffer.width / OutputBuffer.height;
            var gap = aspect2 / aspect1;

            var scale = new Vector2(gap, vflip ? -1 : 1);
            var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);

            Graphics.Blit(source, OutputBuffer, scale, offset);
        }

        void Start()
        {
            // Allocate a render texture if no output texture has been given.
            if (_outputTexture == null)
                _buffer = new RenderTexture
                  (_outputResolution.x, _outputResolution.y, 0);


            if (_materialName.mainTexture != null)
            {
                Blit(_materialName.mainTexture);
            }
        }

        void OnDestroy()
        {
            if (_webcam != null) Destroy(_webcam);
            if (_buffer != null) Destroy(_buffer);
            if (_material != null) Destroy(_material);
        }


        private void Update()
        {
            if (_materialName.mainTexture != null)
            {
                Blit(_materialName.mainTexture);
            }
        }
    }

}