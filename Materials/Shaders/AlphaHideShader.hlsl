 Shader "AlphaHideShader" 
 {
	 Properties 
	 {
		 _MainTex ("Base", 2D) = "white" {}
		 _MainTexA("BaseA", 2D) = "red" {}
	 }
 
	 SubShader 
	 {
		 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		 LOD 100
	 
		 ZWrite Off
		 Blend SrcAlpha OneMinusSrcAlpha 
 
		 Pass 
		 {
			 Lighting Off
			 SetTexture [_MainTex] 
			 { 
				 Combine texture
			 }
			 SetTexture [_MainTexA]
			 {
				 Combine texture * previous		 
			 }
		 }
	 }
 }