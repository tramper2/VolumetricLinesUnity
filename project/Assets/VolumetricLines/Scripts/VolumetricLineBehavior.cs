using UnityEngine;
using System.Collections;
using VolumetricLines.Utils;

namespace VolumetricLines
{
	/// <summary>
	/// Render a single volumetric line
	/// 
	/// Based on the Volumetric lines algorithm by Sebastien Hillaire
	/// http://sebastien.hillaire.free.fr/index.php?option=com_content&view=article&id=57&Itemid=74
	/// 
	/// Thread in the Unity3D Forum:
	/// http://forum.unity3d.com/threads/181618-Volumetric-lines
	/// 
	/// Unity3D port by Johannes Unterguggenberger
	/// johannes.unterguggenberger@gmail.com
	/// 
	/// Thanks to Michael Probst for support during development.
	/// 
	/// Thanks for bugfixes and improvements to Unity Forum User "Mistale"
	/// http://forum.unity3d.com/members/102350-Mistale
    /// 
    /// Shader code optimization and cleanup by Lex Darlog (aka DRL)
    /// http://forum.unity3d.com/members/lex-drl.67487/
    /// 
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(Renderer))]
	[ExecuteInEditMode]
	public class VolumetricLineBehavior : MonoBehaviour 
	{
		#region private variables
		/// <summary>
		/// Template material to be used
		/// </summary>
		[SerializeField]
		public Material m_templateMaterial;

		/// <summary>
		/// Set to false in order to change the material's properties as specified in this script.
		/// Set to true in order to *initially* leave the material's properties as they are in the template material.
		/// </summary>
		[SerializeField] 
		private bool m_doNotOverwriteTemplateMaterialProperties;

		/// <summary>
		/// The start position relative to the GameObject's origin
		/// </summary>
		[SerializeField] 
		private Vector3 m_startPos;
		
		/// <summary>
		/// The end position relative to the GameObject's origin
		/// </summary>
		[SerializeField] 
		private Vector3 m_endPos = new Vector3(0f, 0f, 100f);

		/// <summary>
		/// Line Color
		/// </summary>
		[SerializeField] 
		private Color m_lineColor;

		/// <summary>
		/// The width of the line
		/// </summary>
		[SerializeField] 
		private float m_lineWidth;

		private Material m_material;
		#endregion

		#region properties

		public Color LineColor
		{
			get { return m_lineColor;  }
			set
			{
				m_lineColor = value;
				m_material.color = m_lineColor;
			}
		}

		public float LineWidth
		{
			get { return m_lineWidth; }
			set
			{
				m_lineWidth = value;
				m_material.SetFloat("_LineWidth", m_lineWidth);
			}
		}

		public Vector3 StartPos
		{
			get { return m_startPos; }
			set
			{
				m_startPos = value;
				SetStartAndEndPoints(m_startPos, m_endPos);
			}
		}

		public Vector3 EndPos
		{
			get { return m_endPos; }
			set
			{
				m_endPos = value;
				SetStartAndEndPoints(m_startPos, m_endPos);
			}
		}

		#endregion
		
		#region methods
		private void CreateMaterial()
		{
			if (null != m_templateMaterial && null == m_material)
			{
				m_material = Material.Instantiate(m_templateMaterial);
				GetComponent<Renderer>().sharedMaterial = m_material;
				SetAllMaterialProperties();
			}
		}

		private void DestroyMaterial()
		{
			if (null != m_material)
			{
				DestroyImmediate(m_material);
				m_material = null;
			}
		}

		private void SetAllMaterialProperties()
		{
			SetStartAndEndPoints(m_startPos, m_endPos);

			if (null != m_material)
			{
				if (!m_doNotOverwriteTemplateMaterialProperties)
				{
					m_material.color = m_lineColor;
					m_material.SetFloat("_LineWidth", m_lineWidth);
				}

				m_material.SetFloat("_LineScale", transform.GetGlobalUniformScaleForLineWidth());
			}
		}

		/// <summary>
		/// Sets the start and end points - updates the data of the Mesh.
		/// </summary>
		public void SetStartAndEndPoints(Vector3 startPoint, Vector3 endPoint)
		{
			Vector3[] vertexPositions = {
				startPoint,
				startPoint,
				startPoint,
				startPoint,
				endPoint,
				endPoint,
				endPoint,
				endPoint,
			};
			
			Vector3[] other = {
				endPoint,
				endPoint,
				endPoint,
				endPoint,
				startPoint,
				startPoint,
				startPoint,
				startPoint,
			};
			
			var mesh = GetComponent<MeshFilter>().sharedMesh;
			if (null != mesh)
			{
				mesh.vertices = vertexPositions;
				mesh.normals = other;
                mesh.RecalculateBounds();
			}
		}
		#endregion

		#region event functions
		// Vertex data is updated only in Start() unless m_dynamic is set to true
		void Start () 
		{
			Vector3[] vertexPositions = {
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
			};
			
			Vector3[] other = {
				m_endPos,
				m_endPos,
				m_endPos,
				m_endPos,
				m_startPos,
				m_startPos,
				m_startPos,
				m_startPos,
			};
			
			// Need to set vertices before assigning new Mesh to the MeshFilter's mesh property
			Mesh mesh = new Mesh();
			mesh.vertices = vertexPositions;
			mesh.normals = other;
			mesh.uv = VolumetricLineVertexData.TexCoords;
			mesh.uv2 = VolumetricLineVertexData.VertexOffsets;
			mesh.SetIndices(VolumetricLineVertexData.Indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
			GetComponent<MeshFilter>().mesh = mesh;
			CreateMaterial();
		}

		void OnDestroy()
		{
			DestroyMaterial();
		}
		
		void Update()
		{
			if (transform.hasChanged && null != m_material)
			{
				m_material.SetFloat("_LineScale", transform.GetGlobalUniformScaleForLineWidth());
			}
		}

		void OnValidate()
		{
			// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
			//  => make sure, everything stays up-to-date
			SetAllMaterialProperties();
		}
	
		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(gameObject.transform.TransformPoint(m_startPos), gameObject.transform.TransformPoint(m_endPos));
		}
		#endregion
	}
}