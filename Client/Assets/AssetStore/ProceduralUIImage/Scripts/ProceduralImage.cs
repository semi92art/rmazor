namespace UnityEngine.UI.ProceduralImage
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Procedural Image")]
    public class ProceduralImage : Image
    {
        [SerializeField] private float borderWidth;
        private ProceduralImageModifier m_Modifier;
        private static Material _materialInstance;
        private static Material DefaultProceduralImageMaterial
        {
            get
            {
                if (_materialInstance == null)
                {
                    _materialInstance = new Material(Shader.Find("UI/Procedural UI Image"));
                }
                return _materialInstance;
            }
            set => _materialInstance = value;
        }
        [SerializeField] private float falloffDistance = 1;

        public float BorderWidth
        {
            get => borderWidth;
            set
            {
                borderWidth = value;
                SetVerticesDirty();
            }
        }

        public float FalloffDistance
        {
            get => falloffDistance;
            set
            {
                falloffDistance = value;
                SetVerticesDirty();
            }
        }

        private ProceduralImageModifier Modifier
        {
            get
            {
                if (m_Modifier != null)
                    return m_Modifier;
                //try to get the modifier on the object.
                m_Modifier = GetComponent<ProceduralImageModifier>();
                //if we did not find any modifier
                if (m_Modifier == null)
                {
                    //Add free modifier
                    ModifierType = typeof(FreeModifier);
                }
                return m_Modifier;
            }
            set => m_Modifier = value;
        }

        /// <summary>
        /// Gets or sets the type of the modifier. Adds a modifier of that type.
        /// </summary>
        /// <value>The type of the modifier.</value>
        public System.Type ModifierType
        {
            get => Modifier.GetType();
            set
            {
                if (m_Modifier != null && m_Modifier.GetType() != value)
                {
                    if (GetComponent<ProceduralImageModifier>() != null)
                    {
                        DestroyImmediate(GetComponent<ProceduralImageModifier>());
                    }
                    gameObject.AddComponent(value);
                    Modifier = GetComponent<ProceduralImageModifier>();
                    SetAllDirty();
                }
                else if (m_Modifier == null)
                {
                    gameObject.AddComponent(value);
                    Modifier = GetComponent<ProceduralImageModifier>();
                    SetAllDirty();
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Init();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_OnDirtyVertsCallback -= OnVerticesDirty;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            FixTexCoordsInCanvas();
            m_OnDirtyVertsCallback += OnVerticesDirty;
            preserveAspect = false;
            material = null;
            if (sprite == null)
            {
                sprite = EmptySprite.Get();
            }
        }

        private void OnVerticesDirty()
        {
            if (sprite == null)
            {
                sprite = EmptySprite.Get();
            }
        }

        private void FixTexCoordsInCanvas()
        {
            Canvas c = GetComponentInParent<Canvas>();
            if (c != null)
            {
                FixTexCoordsInCanvas(c);
            }
        }

        private static void FixTexCoordsInCanvas(Canvas _C)
        {
            _C.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1
                                          | AdditionalCanvasShaderChannels.TexCoord2
                                          | AdditionalCanvasShaderChannels.TexCoord3;
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateGeometry();
            }
        }
#endif

        /// <summary>
        /// Prevents radius to get bigger than rect size
        /// </summary>
        /// <returns>The fixed radius.</returns>
        /// <param name="_Vec">border-radius as Vector4 (starting upper-left, clockwise)</param>
        private Vector4 FixRadius(Vector4 _Vec)
        {
            Rect r = rectTransform.rect;
            _Vec = new Vector4(Mathf.Max(_Vec.x, 0), Mathf.Max(_Vec.y, 0), Mathf.Max(_Vec.z, 0), Mathf.Max(_Vec.w, 0));

            //Allocates mem
            //float scaleFactor = Mathf.Min(r.width / (vec.x + vec.y), r.width / (vec.z + vec.w), r.height / (vec.x + vec.w), r.height / (vec.z + vec.y), 1);
            //Allocation free:
            float scaleFactor = Mathf.Min (Mathf.Min (Mathf.Min (Mathf.Min (r.width / (_Vec.x + _Vec.y), r.width / (_Vec.z + _Vec.w)), r.height / (_Vec.x + _Vec.w)), r.height / (_Vec.z + _Vec.y)), 1f);
            return _Vec * scaleFactor;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            EncodeAllInfoIntoVertices(toFill, CalculateInfo());
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            FixTexCoordsInCanvas();
        }

        private ProceduralImageInfo CalculateInfo()
        {
            var r = GetPixelAdjustedRect();
            float pixelSize = 1f / Mathf.Max(0, falloffDistance);

            Vector4 radius = FixRadius(Modifier.CalculateRadius(r));

            float minside = Mathf.Min(r.width, r.height);

            ProceduralImageInfo info = new ProceduralImageInfo(r.width + falloffDistance, r.height + falloffDistance, falloffDistance, pixelSize, radius / minside, borderWidth / minside * 2);

            return info;
        }

        private void EncodeAllInfoIntoVertices(VertexHelper vh, ProceduralImageInfo info)
        {
            UIVertex vert = new UIVertex();

            Vector2 uv1 = new Vector2(info.width, info.height);
            Vector2 uv2 = new Vector2(EncodeFloats_0_1_16_16(info.radius.x, info.radius.y), EncodeFloats_0_1_16_16(info.radius.z, info.radius.w));
            Vector2 uv3 = new Vector2(info.borderWidth == 0 ? 1 : Mathf.Clamp01(info.borderWidth), info.pixelSize);

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);

                vert.position += ((Vector3)vert.uv0 - new Vector3(0.5f, 0.5f)) * info.fallOffDistance;
                //vert.uv0 = vert.uv0;
                vert.uv1 = uv1;
                vert.uv2 = uv2;
                vert.uv3 = uv3;

                vh.SetUIVertex(vert, i);
            }
        }

        /// <summary>
        /// Encode two values between [0,1] into a single float. Each using 16 bits.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static float EncodeFloats_0_1_16_16(float a, float b)
        {
            Vector2 kDecodeDot = new Vector2(1.0f, 1f / 65535.0f);
            return Vector2.Dot(new Vector2(Mathf.Floor(a * 65534) / 65535f, Mathf.Floor(b * 65534) / 65535f), kDecodeDot);
        }

        public override Material material
        {
            get => m_Material == null ? DefaultProceduralImageMaterial : base.material;

            set => base.material = value;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            OnEnable();
        }

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();

            //Don't allow negative numbers for fall off distance
            falloffDistance = Mathf.Max(0, falloffDistance);

            //Don't allow negative numbers for fall off distance
            borderWidth = Mathf.Max(0, borderWidth);
        }
#endif
    }

    /// <summary>
    /// Contains all parameters of a proceduaral image
    /// </summary>
    public struct ProceduralImageInfo
    {
        public float width;
        public float height;
        public float fallOffDistance;
        public Vector4 radius;
        public float borderWidth;
        public float pixelSize;

        public ProceduralImageInfo(float _Width, float _Height, float _FallOffDistance, float _PixelSize, Vector4 _Radius, float _BorderWidth)
        {
            this.width = Mathf.Abs(_Width);
            this.height = Mathf.Abs(_Height);
            this.fallOffDistance = Mathf.Max(0, _FallOffDistance);
            this.radius = _Radius;
            this.borderWidth = Mathf.Max(_BorderWidth, 0);
            this.pixelSize = Mathf.Max(0, _PixelSize);
        }
    }
}
