Shader "Custom/Sprite2DOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1.0
        _OutlineSoftness ("Outline Softness", Range(0, 1)) = 0.5
        [MaterialToggle] _UseAlpha ("Use Alpha Channel", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;
            float _UseAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Get the alpha threshold based on original texture
                float alphaThreshold = _UseAlpha > 0.5 ? 0.01 : col.a * 0.01;
                
                // Calculate outline by sampling adjacent pixels with anti-aliasing
                float outlineAlpha = 0;
                float maxOutlineAlpha = 0;
                float sampleCount = 0;
                float2 pixelSize = _OutlineWidth * _MainTex_TexelSize.xy;
                float sqrt2_2 = 0.707106781;
                
                // Sample in 8 directions with different distances for anti-aliasing
                float2 directions[8] = {
                    float2(1, 0), float2(-1, 0),     // right, left
                    float2(0, 1), float2(0, -1),     // up, down
                    float2(sqrt2_2, sqrt2_2), float2(-sqrt2_2, sqrt2_2),     // diagonal up
                    float2(sqrt2_2, -sqrt2_2), float2(-sqrt2_2, -sqrt2_2)    // diagonal down
                };
                
                // Multiple samples per direction for better anti-aliasing
                const int SAMPLES_PER_DIRECTION = 3;
                
                for (int dirIdx = 0; dirIdx < 8; dirIdx++) {
                    for (int sampleIdx = 1; sampleIdx <= SAMPLES_PER_DIRECTION; sampleIdx++) {
                        // Calculate distance for current sample
                        float dist = float(sampleIdx) / float(SAMPLES_PER_DIRECTION);
                        float2 sampleOffset = directions[dirIdx] * pixelSize * dist;
                        
                        // Sample the texture
                        float sampleAlpha = tex2D(_MainTex, i.uv + sampleOffset).a;
                        
                        // Track max alpha (for strength) and accumulate alpha
                        maxOutlineAlpha = max(maxOutlineAlpha, sampleAlpha);
                        outlineAlpha += sampleAlpha;
                        sampleCount += 1.0;
                    }
                }
                
                // Normalize the accumulated alpha
                outlineAlpha /= sampleCount;
                
                // Scale the outline alpha based on distance from edges
                float finalOutlineStrength = 0;
                if (col.a < alphaThreshold && maxOutlineAlpha > 0) {
                    // Create smooth outlines based on alpha gradient with adjustable softness
                    float softnessFactor = lerp(0.9, 0.1, _OutlineSoftness);
                    finalOutlineStrength = smoothstep(0.0, softnessFactor, outlineAlpha);
                }
                
                // Blend the original color with the outline color
                fixed4 outlineColor = _OutlineColor;
                
                // Apply outline alpha as a function of strength and outline color alpha
                outlineColor.a *= finalOutlineStrength * _OutlineColor.a;
                
                // If we're completely within the original sprite, use original color
                if (col.a >= alphaThreshold) {
                    // Apply original color with premultiplied alpha
                    col.rgb *= col.a;
                    return col;
                }
                
                // If we're in the outline area, blend based on outline strength
                if (outlineColor.a > 0) {
                    return outlineColor;
                }
                
                // Return transparent for areas outside both sprite and outline
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
    CustomEditor "OutlineShaderGUI"
}