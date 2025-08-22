Shader "Custom/VoronoiOnSphere"
{
    Properties
    {
        _PointCount("Point Count", Int) = 5
        _Radius("Radius", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            uniform int _PointCount;
            uniform float4 _PointSphericalCoords[1028]; // Maximum of 128 points
            uniform fixed4 _Colors[1028];
            uniform float _Radius;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0; // Using local position instead of world position
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz; // Store local position
                return o;
            }

            float3 SphericalToEuclidean(float4 spherical)
            {
                float radius = spherical.x;
                float theta = spherical.y;
                float phi = spherical.z;

                float x = radius * sin(phi) * cos(theta);
                float y = radius * sin(phi) * sin(theta);
                float z = radius * cos(phi);

                return float3(x, y, z);
            }
        
            float3 EuclideanToSpherical(float3 euclidean)
            {
                float radius = length(euclidean);
                float theta = atan2(euclidean.y, euclidean.x);
                float phi = acos(euclidean.z / radius);
            
                return float3(radius, theta, phi);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 worldPos = i.localPos;

                float PI = radians(180);

                // Initialize minimum distance and corresponding color
                float minDist = 1e20;
                fixed4 finalColor = fixed4(1, 1, 1, 1); // Default color
                float3 sphericalWorldPos = EuclideanToSpherical(worldPos);

                for (int j = 0; j < _PointCount; j++) 
                {

                    // define points
                    float3 sphericalPointPos = _PointSphericalCoords[j].xyz;
                    float3 euclideanPointPos = SphericalToEuclidean(_PointSphericalCoords[j]);

                    
                    // euclidean distance
                    float distance = sqrt( pow(worldPos.x - euclideanPointPos.x, 2) + pow(worldPos.y - euclideanPointPos.y,2) + pow(worldPos.z - euclideanPointPos.z,2) );


                    // spherical distance
                    distance = 0;
                    float angularDistancePhi = abs( sphericalPointPos.z - sphericalWorldPos.z );            

                    // check for angular distance:
                    // if <= 2 calc direct dist, else calc shortest dist over poles
                    if ( angularDistancePhi <= 2)
                    {
                        // calculate direct distance only moving along longitute half-circles and latitude circles
                        float minLatCircle = _Radius * min(  sin(sphericalPointPos.z), sin(sphericalWorldPos.z) );
                        float angularDistanceTheta = abs(min( abs(sphericalPointPos.y - sphericalWorldPos.y), 2 * PI - abs(sphericalPointPos.y - sphericalWorldPos.y) ));

                        distance = minLatCircle * angularDistanceTheta +  _Radius *  angularDistancePhi ;     
                    }
                    else 
                    {
                        // calculate shortest distance over one of the poles along longitude half-circles

                        float distanceOverNorthPole = sphericalWorldPos.z + sphericalPointPos.z;
                        float distanceOverSouthPole = abs( PI - sphericalWorldPos.z) + abs( PI - sphericalPointPos.z);

                        distance = _Radius * min( distanceOverNorthPole, distanceOverSouthPole );
                    }


                    if (distance < minDist)
                    {
                        minDist = distance;
                        finalColor = _Colors[j];
                    }
                }

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
