using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtAurora))]
public class SgtAurora_Editor : SgtEditor<SgtAurora>
{
	protected override void OnInspector()
	{
		var updateMaterial        = false;
		var updateMeshesAndModels = false;

		DrawDefault("Color", ref updateMaterial);
		DrawDefault("Brightness", ref updateMaterial);
		DrawDefault("RenderQueue", ref updateMaterial);
		DrawDefault("RenderQueueOffset", ref updateMaterial);
		DrawDefault("CameraOffset"); // Updated automatically

		Separator();
		
		BeginError(Any(t => t.MainTex == null));
			DrawDefault("MainTex", ref updateMaterial);
		EndError();
		DrawDefault("Seed", ref updateMeshesAndModels);
		BeginError(Any(t => t.RadiusMin >= t.RadiusMax));
			DrawDefault("RadiusMin", ref updateMaterial);
			DrawDefault("RadiusMax", ref updateMaterial);
		EndError();

		Separator();

		BeginError(Any(t => t.PathCount < 1));
			DrawDefault("PathCount", ref updateMeshesAndModels);
		EndError();
		BeginError(Any(t => t.PathDetail < 1));
			DrawDefault("PathDetail", ref updateMeshesAndModels);
		EndError();
		BeginError(Any(t => t.PathLengthMin > t.PathLengthMax));
			DrawDefault("PathLengthMin", ref updateMeshesAndModels);
			DrawDefault("PathLengthMax", ref updateMeshesAndModels);
		EndError();

		Separator();

		BeginError(Any(t => t.StartMin > t.StartMax));
			DrawDefault("StartMin", ref updateMeshesAndModels);
			DrawDefault("StartMax", ref updateMeshesAndModels);
		EndError();
		BeginError(Any(t => t.StartBias < 1.0f));
			DrawDefault("StartBias", ref updateMeshesAndModels);
		EndError();
		DrawDefault("StartTop", ref updateMeshesAndModels);
		
		Separator();
		
		DrawDefault("PointDetail", ref updateMeshesAndModels);
		DrawDefault("PointSpiral", ref updateMeshesAndModels);
		DrawDefault("PointJitter", ref updateMeshesAndModels);

		Separator();

		DrawDefault("TrailTile", ref updateMeshesAndModels);
		BeginError(Any(t => t.TrailEdgeFade < 1.0f));
			DrawDefault("TrailEdgeFade", ref updateMeshesAndModels);
		EndError();
		DrawDefault("TrailHeights", ref updateMeshesAndModels);
		BeginError(Any(t => t.TrailHeightsDetail < 1));
			DrawDefault("TrailHeightsDetail", ref updateMeshesAndModels);
		EndError();

		Separator();
		
		DrawDefault("Colors", ref updateMeshesAndModels);
		BeginError(Any(t => t.ColorsDetail < 1));
			DrawDefault("ColorsDetail", ref updateMeshesAndModels);
		EndError();
		DrawDefault("ColorsAlpha", ref updateMeshesAndModels);
		DrawDefault("ColorsAlphaBias", ref updateMeshesAndModels);

		Separator();
		
		DrawDefault("FadeNear", ref updateMaterial);
		
		if (Any(t => t.FadeNear == true))
		{
			BeginIndent();
				BeginError(Any(t => t.FadeNearTex == null));
					DrawDefault("FadeNearTex", ref updateMaterial);
				EndError();
				BeginError(Any(t => t.FadeNearRadius < 0.0f));
					DrawDefault("FadeNearRadius", ref updateMaterial);
				EndError();
				BeginError(Any(t => t.FadeNearThickness <= 0.0f));
					DrawDefault("FadeNearThickness", ref updateMaterial);
				EndError();
			EndIndent();
		}

		Separator();
		
		DrawDefault("Anim", ref updateMaterial);
		
		if (Any(t => t.Anim == true))
		{
			BeginIndent();
				DrawDefault("AnimOffset"); // Updated automatically
				BeginError(Any(t => t.AnimSpeed == 0.0f));
					DrawDefault("AnimSpeed"); // Updated automatically
				EndError();
				DrawDefault("AnimStrength", ref updateMeshesAndModels);
				BeginError(Any(t => t.AnimStrengthDetail < 1));
					DrawDefault("AnimStrengthDetail", ref updateMeshesAndModels);
				EndError();
				DrawDefault("AnimAngle", ref updateMeshesAndModels);
				BeginError(Any(t => t.AnimAngleDetail < 1));
					DrawDefault("AnimAngleDetail", ref updateMeshesAndModels);
				EndError();
			EndIndent();
		}

		if (updateMaterial        == true) DirtyEach(t => t.UpdateMaterial       ());
		if (updateMeshesAndModels == true) DirtyEach(t => t.UpdateMeshesAndModels());

		if (Any(t => t.MainTex == null && t.GetComponent<SgtAuroraMainTex>() == null))
		{
			Separator();

			if (Button("Add Main Tex") == true)
			{
				Each(t => SgtHelper.GetOrAddComponent<SgtAuroraMainTex>(t.gameObject));
			}
		}

		if (Any(t => t.FadeNear == true && t.FadeNearTex == null && t.GetComponent<SgtAuroraFadeNear>() == null))
		{
			Separator();

			if (Button("Add Fade Near") == true)
			{
				Each(t => SgtHelper.GetOrAddComponent<SgtAuroraFadeNear>(t.gameObject));
			}
		}
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Aurora")]
public class SgtAurora : MonoBehaviour
{
	[Tooltip("The main texture")]
	public Texture MainTex;

	[Tooltip("The color tint")]
	public Color Color = Color.white;

	[Tooltip("The Color.rgb values are multiplied by this")]
	public float Brightness = 1.0f;

	[Tooltip("The render queue group of the aurora material")]
	public SgtRenderQueue RenderQueue = SgtRenderQueue.Transparent;

	[Tooltip("The render queue offset of the aurora material")]
	public int RenderQueueOffset;
	
	[Tooltip("The distance the aurora mesh is moved toward the rendering camera in world space")]
	public float CameraOffset;

	[Tooltip("The random seed used when generating the aurora mesh")]
	[SgtSeed]
	public int Seed;

	[Tooltip("The inner radius of the aurora mesh in local space")]
	public float RadiusMin = 1.0f;

	[Tooltip("The outer radius of the aurora mesh in local space")]
	public float RadiusMax = 1.1f;
	
	[Tooltip("The amount of aurora paths")]
	public int PathCount = 8;

	[Tooltip("The detail of each aurora path")]
	public int PathDetail = 100;
	
	[Tooltip("The sharpness of the fading at the start and ends of the aurora paths")]
	public float TrailEdgeFade = 1.0f;

	[Tooltip("The minimum length of each aurora path")]
	[Range(0.0f, 1.0f)]
	public float PathLengthMin = 0.1f;
	
	[Tooltip("The maximum length of each aurora path")]
	[Range(0.0f, 1.0f)]
	public float PathLengthMax = 0.1f;
	
	[Tooltip("The minimum distance between the pole at the aurora path start point")]
	[Range(0.0f, 1.0f)]
	public float StartMin = 0.1f;
	
	[Tooltip("The maximum distance between the pole at the aurora path start point")]
	[Range(0.0f, 1.0f)]
	public float StartMax = 0.5f;

	[Tooltip("The probability that the aurora path will begin closer to the pole")]
	public float StartBias = 1.0f;

	[Tooltip("The probability that the aurora path will start on the northern pole")]
	[Range(0.0f, 1.0f)]
	public float StartTop = 0.5f;
	
	[Tooltip("The amount of waypoints the aurora path will follow based on its length")]
	[Range(1, 100)]
	public int PointDetail = 10;
	
	[Tooltip("The strength of the aurora waypoint twisting")]
	public float PointSpiral = 1.0f;

	[Tooltip("The strength of the aurora waypoint random displacement")]
	[Range(0.0f, 1.0f)]
	public float PointJitter = 1.0f;
	
	[Tooltip("The amount of times the main texture is tiled based on its length")]
	public float TrailTile = 10.0f;
	
	[Tooltip("The flatness of the aurora path")]
	[Range(0.1f, 1.0f)]
	public float TrailHeights = 1.0f;

	[Tooltip("The amount of height changes in the aurora path")]
	public int TrailHeightsDetail = 10;
	
	[Tooltip("The possible colors given to the top half of the aurora path")]
	public Gradient Colors;

	[Tooltip("The amount of color changes an aurora path can have based on its length")]
	public int ColorsDetail = 10;

	[Tooltip("The minimum opacity multiplier of the aurora path colors")]
	[Range(0.0f, 1.0f)]
	public float ColorsAlpha = 0.5f;
	
	[Tooltip("The amount of alpha changes in the aurora path")]
	public float ColorsAlphaBias = 2.0f;
	
	[Tooltip("Should the aurora fade out when the camera gets near?")]
	public bool FadeNear;

	[Tooltip("The lookup table used to calculate the fading amount based on the distance")]
	public Texture FadeNearTex;

	[Tooltip("The radius of the fading effect in world space")]
	public float FadeNearRadius = 2.0f;

	[Tooltip("The thickness of the fading effect in world space")]
	public float FadeNearThickness = 2.0f;
	
	[Tooltip("Enabled aurora path animation")]
	public bool Anim;

	[Tooltip("The current age/offset of the animation")]
	public float AnimOffset;

	[Tooltip("The speed of the animation")]
	public float AnimSpeed = 1.0f;

	[Tooltip("The strength of the aurora path position changes in local space")]
	public float AnimStrength = 0.01f;

	[Tooltip("The amount of the animation strength changes along the aurora path based on its length")]
	public int AnimStrengthDetail = 10;

	[Tooltip("The maximum angle step between sections of the aurora path")]
	public float AnimAngle = 0.01f;

	[Tooltip("The amount of the animation angle changes along the aurora path based on its length")]
	public int AnimAngleDetail = 10;

	// The models used to render this
	public List<SgtAuroraModel> Models;
	
	// The material applied to all segments
	[System.NonSerialized]
	public Material Material;

	// The meshes applied to the models
	[System.NonSerialized]
	public List<Mesh> Meshes;

	[SerializeField]
	private bool startCalled;

	[System.NonSerialized]
	private bool updateMaterialCalled;

	[System.NonSerialized]
	protected bool updateMeshesAndModelsCalled;

	private static List<Vector3> positions = new List<Vector3>();

	private static List<Vector4> coords0 = new List<Vector4>();
	
	private static List<Color> colors = new List<Color>();
	
	private static List<Vector3> normals = new List<Vector3>();

	private static List<int> indices = new List<int>();
	
	public void UpdateMainTex()
	{
		if (Material != null)
		{
			Material.SetTexture("_MainTex", MainTex);
		}
	}

	public void UpdateFadeNearTex()
	{
		if (Material != null)
		{
			Material.SetTexture("_FadeNearTex", FadeNearTex);
		}
	}

	[ContextMenu("Update Material")]
	public void UpdateMaterial()
	{
		updateMaterialCalled = true;

		if (Material == null)
		{
			Material = SgtHelper.CreateTempMaterial("Aurora (Generated)", SgtHelper.ShaderNamePrefix + "Aurora");

			if (Models != null)
			{
				for (var i = Models.Count - 1; i >= 0; i--)
				{
					var model = Models[i];

					if (model != null)
					{
						model.SetMaterial(Material);
					}
				}
			}
		}

		var color       = SgtHelper.Premultiply(SgtHelper.Brighten(Color, Brightness));
		var renderQueue = (int)RenderQueue + RenderQueueOffset;

		if (Material.renderQueue != renderQueue)
		{
			Material.renderQueue = renderQueue;
		}
		
		Material.SetColor("_Color", color);
		Material.SetTexture("_MainTex", MainTex);
		Material.SetFloat("_RadiusMin", RadiusMin);
		Material.SetFloat("_RadiusSize", RadiusMax - RadiusMin);

		SgtHelper.SetTempMaterial(Material);
		
		if (FadeNear == true)
		{
			SgtHelper.EnableKeyword("SGT_A"); // FadeNear

			Material.SetTexture("_FadeNearTex", FadeNearTex);
			Material.SetFloat("_FadeNearRadius", FadeNearRadius);
			Material.SetFloat("_FadeNearScale", SgtHelper.Reciprocal(FadeNearThickness));
		}
		else
		{
			SgtHelper.DisableKeyword("SGT_A"); // FadeNear
		}

		if (Anim == true)
		{
			SgtHelper.EnableKeyword("SGT_B"); // Anim

			Material.SetFloat("_AnimOffset", AnimOffset);
		}
		else
		{
			SgtHelper.DisableKeyword("SGT_B"); // Anim
		}
	}

	private void BakeMesh(Mesh mesh)
	{
		mesh.Clear(false);
		mesh.SetVertices(positions);
		mesh.SetUVs(0, coords0);
		mesh.SetColors(colors);
		mesh.SetNormals(normals);
		mesh.SetTriangles(indices, 0);

		mesh.bounds = new Bounds(Vector3.zero, Vector3.one * RadiusMax * 2.0f);
	}

	private Mesh GetMesh(int index)
	{
		SgtHelper.ClearCapacity(positions, 1024);
		SgtHelper.ClearCapacity(coords0, 1024);
		SgtHelper.ClearCapacity(indices, 1024);
		SgtHelper.ClearCapacity(colors, 1024);
		SgtHelper.ClearCapacity(normals, 1024);

		if (index >= Meshes.Count)
		{
			var newMesh = SgtHelper.CreateTempMesh("Aurora Mesh (Generated)");

			Meshes.Add(newMesh);

			return newMesh;
		}

		var mesh = Meshes[index];

		if (mesh == null)
		{
			mesh = Meshes[index] = SgtHelper.CreateTempMesh("Aurora Mesh (Generated)");
		}

		return mesh;
	}

	private Vector3 GetStart(float angle)
	{
		var distance = Mathf.Lerp(StartMin, StartMax, Mathf.Pow(Random.value, StartBias));

		if (Random.value < StartTop)
		{
			return new Vector3(Mathf.Sin(angle) * distance, 1.0f, Mathf.Cos(angle) * distance);
		}
		else
		{
			return new Vector3(Mathf.Sin(angle) * distance, -1.0f, Mathf.Cos(angle) * distance);
		}
	}

	private Vector3 GetNext(Vector3 point, float angle, float speed)
	{
		var noise = Random.insideUnitCircle;
		
		point.x += Mathf.Sin(angle) * speed;
		point.z += Mathf.Cos(angle) * speed;

		point.x += noise.x * PointJitter;
		point.z += noise.y * PointJitter;

		return Quaternion.Euler(0.0f, PointSpiral, 0.0f) * point;
	}
	
	private float GetNextAngle(float angle)
	{
		return angle + Random.Range(0.0f, AnimAngle);
	}

	private float GetNextStrength()
	{
		return Random.Range(-AnimStrength, AnimStrength);
	}

	private Color GetNextColor()
	{
		var color = Color.white;

		if (Colors != null)
		{
			color = Colors.Evaluate(Random.value);
		}
		
		color.a *= Mathf.LerpUnclamped(ColorsAlpha, 1.0f, Mathf.Pow(Random.value, ColorsAlphaBias));

		return color;
	}

	private float GetNextHeight()
	{
		return Random.Range(0.0f, TrailHeights);
	}

	private void Shift<T>(ref T a, ref T b, ref T c, T d, ref float f)
	{
		a  = b;
		b  = c;
		c  = d;
		f -= 1.0f;
	}

	private void AddPath(ref Mesh mesh, ref int meshCount, ref int vertexCount)
	{
		var pathLength = Random.Range(PathLengthMin, PathLengthMax);
		var lineCount  = 2 + (int)(pathLength * PathDetail);
		var quadCount  = lineCount - 1;
		var vertices   = quadCount * 2 + 2;

		if (vertexCount + vertices > 65000)
		{
			BakeMesh(mesh);

			mesh        = GetMesh(meshCount);
			meshCount  += 1;
			vertexCount = 0;
		}

		var angle      = Random.Range(-Mathf.PI, Mathf.PI);
		var speed      = 1.0f / PointDetail;
		var detailStep = 1.0f / PathDetail;
		var pointStep  = detailStep * PointDetail;
		var pointFrac  = 0.0f;
		var pointA     = GetStart(angle + Mathf.PI);
		var pointB     = GetNext(pointA, angle, speed);
		var pointC     = GetNext(pointB, angle, speed);
		var pointD     = GetNext(pointC, angle, speed);
		var coordFrac  = 0.0f;
		var edgeFrac   = -1.0f;
		var edgeStep   = 2.0f / lineCount;
		var coordStep  = detailStep * TrailTile;

		var angleA = angle;
		var angleB = GetNextAngle(angleA);
		var angleC = GetNextAngle(angleB);
		var angleD = GetNextAngle(angleC);
		var angleFrac = 0.0f;
		var angleStep = detailStep * AnimAngleDetail;
		
		var strengthA    = 0.0f;
		var strengthB    = GetNextStrength();
		var strengthC    = GetNextStrength();
		var strengthD    = GetNextStrength();
		var strengthFrac = 0.0f;
		var strengthStep = detailStep * AnimStrengthDetail;
		
		var colorA    = GetNextColor();
		var colorB    = GetNextColor();
		var colorC    = GetNextColor();
		var colorD    = GetNextColor();
		var colorFrac = 0.0f;
		var colorStep = detailStep * ColorsDetail;

		var heightA    = GetNextHeight();
		var heightB    = GetNextHeight();
		var heightC    = GetNextHeight();
		var heightD    = GetNextHeight();
		var heightFrac = 0.0f;
		var heightStep = detailStep * TrailHeightsDetail;

		for (var i = 0; i < lineCount; i++)
		{
			while (pointFrac >= 1.0f)
			{
				Shift(ref pointA, ref pointB, ref pointC, pointD, ref pointFrac); pointD = GetNext(pointC, angle, speed);
			}

			while (angleFrac >= 1.0f)
			{
				Shift(ref angleA, ref angleB, ref angleC, angleD, ref angleFrac); angleD = GetNextAngle(angleC);
			}

			while (strengthFrac >= 1.0f)
			{
				Shift(ref strengthA, ref strengthB, ref strengthC, strengthD, ref strengthFrac); strengthD = GetNextStrength();
			}

			while (colorFrac >= 1.0f)
			{
				Shift(ref colorA, ref colorB, ref colorC, colorD, ref colorFrac); colorD = GetNextColor();
			}

			while (heightFrac >= 1.0f)
			{
				Shift(ref heightA, ref heightB, ref heightC, heightD, ref heightFrac); heightD = GetNextHeight();
			}
			
			var point   = SgtHelper.HermiteInterpolate3(pointA, pointB, pointC, pointD, pointFrac);
			var animAng = SgtHelper.HermiteInterpolate(angleA, angleB, angleC, angleD, angleFrac);
			var animStr = SgtHelper.HermiteInterpolate(strengthA, strengthB, strengthC, strengthD, strengthFrac);
			var color   = SgtHelper.HermiteInterpolate(colorA, colorB, colorC, colorD, colorFrac);
			var height  = SgtHelper.HermiteInterpolate(heightA, heightB, heightC, heightD, heightFrac);
			
			// Fade edges
			color.a *= Mathf.SmoothStep(1.0f, 0.0f, Mathf.Pow(Mathf.Abs(edgeFrac), TrailEdgeFade));
			
			coords0.Add(new Vector4(coordFrac, 0.0f, animAng, animStr));
			coords0.Add(new Vector4(coordFrac, height, animAng, animStr));

			positions.Add(point);
			positions.Add(point);
			
			colors.Add(color);
			colors.Add(color);

			pointFrac    += pointStep;
			edgeFrac     += edgeStep;
			coordFrac    += coordStep;
			angleFrac    += angleStep;
			strengthFrac += strengthStep;
			colorFrac    += colorStep;
			heightFrac   += heightStep;
		}

		var vector = positions[1] - positions[0];

		normals.Add(GetNormal(vector, vector));
		normals.Add(GetNormal(vector, vector));

		for (var i = 2; i < lineCount; i++)
		{
			var nextVector = positions[i] - positions[i - 1];

			normals.Add(GetNormal(vector, nextVector));
			normals.Add(GetNormal(vector, nextVector));

			vector = nextVector;
		}
		
		normals.Add(GetNormal(vector, vector));
		normals.Add(GetNormal(vector, vector));

		for (var i = 0; i < quadCount; i++)
		{
			var offset = vertexCount + i * 2;

			indices.Add(offset + 0);
			indices.Add(offset + 1);
			indices.Add(offset + 2);

			indices.Add(offset + 3);
			indices.Add(offset + 2);
			indices.Add(offset + 1);
		}

		vertexCount += vertices;
	}

	private Vector3 GetNormal(Vector3 a, Vector3 b)
	{
		return Vector3.Cross(a.normalized, b.normalized);
	}
	
	[ContextMenu("Update Meshes And Models")]
	public void UpdateMeshesAndModels()
	{
		updateMeshesAndModelsCalled = true;
		
		if (Meshes == null)
		{
			Meshes = new List<Mesh>();
		}

		if (Models == null)
		{
			Models = new List<SgtAuroraModel>();
		}

		if (PathDetail > 0 && PathLengthMin > 0.0f && PathLengthMax > 0.0f)
		{
			var meshCount   = 1;
			var mesh        = GetMesh(0);
			var vertexCount = 0;

			SgtHelper.BeginRandomSeed(Seed);
			{
				for (var i = 0; i < PathCount; i++)
				{
					AddPath(ref mesh, ref meshCount, ref vertexCount);
				}
			}
			SgtHelper.EndRandomSeed();

			BakeMesh(mesh);

			for (var i = Meshes.Count - 1; i >= meshCount; i--)
			{
				var extraMesh = Meshes[i];

				if (extraMesh != null)
				{
					extraMesh.Clear(false);

					SgtObjectPool<Mesh>.Add(extraMesh);
				}

				Meshes.RemoveAt(i);
			}
		}

		for (var i = 0; i < Meshes.Count; i++)
		{
			var model = GetOrAddModel(i);
			
			model.SetMesh(Meshes[i]);
		}
		
		// Remove any excess
		if (Models != null)
		{
			var min = Mathf.Max(0, Meshes.Count);

			for (var i = Models.Count - 1; i >= min; i--)
			{
				SgtAuroraModel.Pool(Models[i]);

				Models.RemoveAt(i);
			}
		}
	}

	public static SgtAurora CreateAurora(int layer = 0, Transform parent = null)
	{
		return CreateAurora(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
	}

	public static SgtAurora CreateAurora(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject = SgtHelper.CreateGameObject("Aurora", layer, parent, localPosition, localRotation, localScale);
		var aurora     = gameObject.AddComponent<SgtAurora>();

		return aurora;
	}

#if UNITY_EDITOR
	[MenuItem(SgtHelper.GameObjectMenuPrefix + "Aurora", false, 10)]
	public static void CreateAuroraMenuItem()
	{
		var parent = SgtHelper.GetSelectedParent();
		var aurora = CreateAurora(parent != null ? parent.gameObject.layer : 0, parent);

		SgtHelper.SelectAndPing(aurora);
	}
#endif

#if UNITY_EDITOR
	protected virtual void Reset()
	{
		if (Colors == null)
		{
			Colors = new Gradient();

			Colors.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.magenta, 1.0f) };
		}
	}
#endif

	protected virtual void OnEnable()
	{
		Camera.onPreCull    += CameraPreCull;
		Camera.onPreRender  += CameraPreRender;
		Camera.onPostRender += CameraPostRender;

		if (Models != null)
		{
			for (var i = Models.Count - 1; i >= 0; i--)
			{
				var plane = Models[i];

				if (plane != null)
				{
					plane.gameObject.SetActive(true);
				}
			}
		}

		if (startCalled == true)
		{
			CheckUpdateCalls();
		}
	}

	protected virtual void Start()
	{
		if (startCalled == false)
		{
			startCalled = true;
			
			CheckUpdateCalls();
		}
	}

	protected virtual void Update()
	{
		if (Anim == true)
		{
			if (Application.isPlaying == true)
			{
				AnimOffset += Time.deltaTime * AnimSpeed;
			}

			if (Material != null)
			{
				Material.SetFloat("_AnimOffset", AnimOffset);
			}
		}
	}

	protected virtual void OnDisable()
	{
		Camera.onPreCull    -= CameraPreCull;
		Camera.onPreRender  -= CameraPreRender;
		Camera.onPostRender -= CameraPostRender;

		if (Models != null)
		{
			for (var i = Models.Count - 1; i >= 0; i--)
			{
				var plane = Models[i];

				if (plane != null)
				{
					plane.gameObject.SetActive(false);
				}
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (Meshes != null)
		{
			for (var i = Meshes.Count - 1; i >= 0; i--)
			{
				var mesh = Meshes[i];

				if (mesh != null)
				{
					mesh.Clear(false);

					SgtObjectPool<Mesh>.Add(Meshes[i]);
				}
			}
		}

		if (Models != null)
		{
			for (var i = Models.Count - 1; i >= 0; i--)
			{
				SgtAuroraModel.MarkForDestruction(Models[i]);
			}
		}
		
		SgtHelper.Destroy(Material);
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		Gizmos.DrawWireSphere(Vector3.zero, RadiusMin);

		Gizmos.DrawWireSphere(Vector3.zero, RadiusMax);
	}
#endif

	private void CameraPreCull(Camera camera)
	{
		if (CameraOffset != 0.0f && Models != null)
		{
			for (var i = Models.Count - 1; i >= 0; i--)
			{
				var plane = Models[i];

				if (plane != null)
				{
					plane.Revert();
					{
						var planeTransform = plane.transform;
						var observerDir    = (planeTransform.position - camera.transform.position).normalized;

						planeTransform.position += observerDir * CameraOffset;
					}
					plane.Save(camera);
				}
			}
		}
	}

	private void CameraPreRender(Camera camera)
	{
		if (Models != null)
		{
			for (var i = Models.Count - 1; i >= 0; i--)
			{
				var plane = Models[i];

				if (plane != null)
				{
					plane.Restore(camera);
				}
			}
		}
	}

	private void CameraPostRender(Camera camera)
	{
		if (Models != null)
		{
			for (var i = Models.Count - 1; i >= 0; i--)
			{
				var plane = Models[i];

				if (plane != null)
				{
					plane.Revert();
				}
			}
		}
	}

	private SgtAuroraModel GetOrAddModel(int index)
	{
		var model = default(SgtAuroraModel);

		if (Models == null)
		{
			Models = new List<SgtAuroraModel>();
		}

		if (index < Models.Count)
		{
			model = Models[index];
		}
		else
		{
			Models.Add(model);
		}

		if (model == null)
		{
			model = Models[index] = SgtAuroraModel.Create(this);

			model.SetMaterial(Material);
		}

		return model;
	}

	private void CheckUpdateCalls()
	{
		if (updateMaterialCalled == false)
		{
			UpdateMaterial();
		}

		if (updateMeshesAndModelsCalled == false)
		{
			UpdateMeshesAndModels();
		}
	}
}