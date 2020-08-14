// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
 
 Shader "UI/DottedLine"
 {
     Properties
     {
        _RepeatCount("Repeat Count", float) = 5
        _Spacing("Spacing", float) = 0.5
        _Offset("Offset", float) = 0

        // these six unused properties are required when a shader
        // is used in the UI system, or you get a warning.
        // look to UI-Default.shader to see these.

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // see for example
        // http://answers.unity3d.com/questions/980924/ui-mask-with-shader.html
     }
     SubShader
     {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
 
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
 
        Pass
        {
            CGPROGRAM

             #pragma vertex vert
             #pragma fragment frag
 
             #include "UnityCG.cginc"
 
             float _RepeatCount;
             float _Spacing;
             float _Offset;
 
             struct appdata
             {
                 float4 vertex : POSITION;
                 float2 uv : TEXCOORD0;
                 fixed4 color : COLOR0;
             };
 
             struct v2f
             {
                 float2 uv : TEXCOORD0;
                 float4 vertex : SV_POSITION;
                 fixed4 color : COLOR0;
             };
 
             v2f vert(appdata v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.uv = v.uv;
                 o.uv.x = (o.uv.x + _Offset) * _RepeatCount * (1.0f + _Spacing);
                 o.color = v.color;
 
                 return o;
             }
 
             fixed4 frag(v2f i) : SV_Target
             {
                 i.uv.x = fmod(i.uv.x, 1.0f + _Spacing);
                 float r = length(i.uv - float2(1.0f + _Spacing, 1.0f) * 0.5f) * 2.0f;
 
                 fixed4 color = i.color;
                 color.a *= saturate((0.99f - r) * 100.0f);
 
                 return color;
             }
            ENDCG
        }
     }
 }
