Shader "TerrainEngine/Diffuse" {
    Properties {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
    }

    CGINCLUDE
        #pragma surface surf Lambert vertex:SplatmapVert finalcolor:SplatmapFinalColor finalprepass:SplatmapFinalPrepass finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog
        #include "TerrainSplatmapCommon.cginc"

        void surf(Input IN, inout SurfaceOutput o)
        {
            half4 splat_control;
            half weight;
            fixed4 mixedDiffuse;
            SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb;
            o.Alpha = weight;
        }
    ENDCG

    Category {
        Tags {
            "Queue" = "Geometry-99"
            "RenderType" = "Opaque"
        }
        // TODO: Seems like "#pragma target 3.0 _NORMALMAP" can't fallback correctly on less capable devices?
        // Use two sub-shaders to simulate different features for different targets and still fallback correctly.
        SubShader { // for sm3.0+ targets
            CGPROGRAM
                #pragma target 3.0
                #pragma multi_compile_local __ _ALPHATEST_ON
                #pragma multi_compile_local __ _NORMALMAP
            ENDCG

            UsePass "Nature/Terrain/Utilities/PICKING"
            UsePass "Nature/Terrain/Utilities/SELECTION"
        }
        SubShader { // for sm2.0 targets
            CGPROGRAM
            ENDCG
        }
    }

    Dependency "AddPassShader"    = "TerrainEngine/Splatmap/Diffuse-AddPass"
    Dependency "BaseMapShader"    = "TerrainEngine/Splatmap/Diffuse-Base"
    Dependency "BaseMapGenShader" = "TerrainEngine/Splatmap/Diffuse-BaseGen"
    Dependency "Details0"         = "TerrainEngine/Details/Vertexlit"
    Dependency "Details1"         = "TerrainEngine/Details/WavingDoublePass"
    Dependency "Details2"         = "TerrainEngine/Details/BillboardWavingDoublePass"
    Dependency "Tree0"            = "TerrainEngine/BillboardTree"

    Fallback "Diffuse"
}
