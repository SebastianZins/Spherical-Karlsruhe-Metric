Shader "Custom/VoronoiOnSphereProjection"
{
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
            uniform StructuredBuffer<float4> _PointSphericalCoords;
            uniform StructuredBuffer<float4> _Colors;
            uniform float _Radius;
            uniform float _ClosestDistance;
            uniform float _MetricType;
            uniform float2 _Scale;
            uniform float _ShowGrid;
            uniform float _MaxDistancePercentage = 100;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz;
                return o;
            }

            float3 SphericalToEuclidean(float3 spherical)
            {
                float radius = spherical.x;
                float theta = spherical.y;
                float phi = spherical.z;

                float x = radius * sin(phi) * cos(theta);
                float y = radius * sin(phi) * sin(theta);
                float z = radius * cos(phi);

                return float3(x, y, z);
            }

            float4 applyGrid(float3 sphericalWorldPos, float4 color) 
            {
                if (_ShowGrid == 1) {
                    bool isGrid = false;
                    if (
                        (round(degrees(sphericalWorldPos.y) * 2) / 2) % 90 == 0 ||
                        (round(degrees(sphericalWorldPos.y) * 4) / 4) % 30 == 0 ||
                        (round(degrees(sphericalWorldPos.y) * 6) / 6) % 10 == 0
                    ) {
                        isGrid = true;
                    }
                    if (
                        (round(degrees(sphericalWorldPos.z) * 2) / 2) % 90 == 0 ||
                        (round(degrees(sphericalWorldPos.z) * 4) / 4) % 30 == 0 ||
                        (round(degrees(sphericalWorldPos.z) * 6) / 6) % 10 == 0
                    ) {
                        isGrid = true;
                    }
                    if (isGrid)
                    {
                        if (color.x == 0 && color.y == 0 && color.z == 0)
                        {
                            return float4(1,1,1,1);
                        }
                        else 
                        {
                            return float4(0,0,0,1);
                        }
                    }
                }
                return color;
            }

            float GetSphericalDistance(float3 sphericalPointPos, float3 sphericalWorldPos)
            {
                float distanceKarlsruhe = 0;
                float angularDistancePhi = abs( sphericalPointPos.z - sphericalWorldPos.z );            

                // check for angular distance:
                // if <= 2 calc direct dist, else calc shortest dist over poles
                if ( angularDistancePhi <= 2)
                {
                    // calculate direct distance only moving along longitute half-circles and latitude circles
                    float minLatCircle = _Radius * min(  sin(sphericalPointPos.z), sin(sphericalWorldPos.z) );
                    float angularDistanceTheta = abs(min( abs(sphericalPointPos.y - sphericalWorldPos.y), 2 * UNITY_PI - abs(sphericalPointPos.y - sphericalWorldPos.y) ));

                    return minLatCircle * angularDistanceTheta +  _Radius *  angularDistancePhi ;     
                }
                else 
                {
                    // calculate shortest distance over one of the poles along longitude half-circles

                    float distanceOverNorthPole = sphericalWorldPos.z + sphericalPointPos.z;
                    float distanceOverSouthPole = abs( UNITY_PI - sphericalWorldPos.z) + abs( UNITY_PI - sphericalPointPos.z);

                    return _Radius * min( distanceOverNorthPole, distanceOverSouthPole );
                }
            }
            
            float GetGeodesicDistance(float3 sphericalPointPos, float3 sphericalWorldPos)
            {
                float phi1 = UNITY_PI / 2 - sphericalWorldPos.z;
                float phi2 = UNITY_PI / 2 - sphericalPointPos.z;

                float deltaSigma = acos(
                    sin(phi1) * sin(phi2) +
                    cos(phi1) * cos(phi2) * cos(sphericalWorldPos.y - sphericalPointPos.y)
                );
                return _Radius * deltaSigma;
            }

            float3 FromMercatorProjectionToSpherical(float3 localPos) 
            {
                // Step 1: Undo Mercator scaling and centering
                float x = (localPos.x + (_Scale.x * 0.25)) / (_Scale.x * 0.5);
                float theta = x * (2.0 * UNITY_PI); // Longitude

                // Step 2: Undo Mercator projection for latitude
                float y = localPos.z;
                float latitude = 2.0 * atan(exp(y)) - UNITY_PI * 0.5;
                float phi = UNITY_PI * 0.5 - latitude; // Polar angle

                return float3(_Radius, theta, phi);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 worldPos = i.localPos;

                float3 sphericalWorldPos = FromMercatorProjectionToSpherical(worldPos);
                float3 euclideanWorldPos = SphericalToEuclidean(sphericalWorldPos);

                // Initialize minimum distance
                float refDistEuclid = _ClosestDistance == 1 ? 1e20 : -1e20;
                float finalColorIndexEuclid = 0;

                float refDistKarlsruhe = _ClosestDistance == 1 ? 1e20 : -1e20;
                float finalColorIndexKarlsruhe = 0;
                
                if (_ShowGrid == 1) {
                    if (
                        (round(degrees(sphericalWorldPos.y) * 2) / 2) % 90 == 0 ||
                        (round(degrees(sphericalWorldPos.y) * 4) / 4) % 30 == 0 ||
                        (round(degrees(sphericalWorldPos.y) * 6) / 6) % 10 == 0
                    ) {
                        return float4(0,0,0,1);
                    }

                    if (
                        (round(degrees(sphericalWorldPos.z) * 2) / 2) % 90 == 0 ||
                        (round(degrees(sphericalWorldPos.z) * 4) / 4) % 30 == 0 ||
                        (round(degrees(sphericalWorldPos.z) * 6) / 6) % 10 == 0
                    ) {
                        return float4(0,0,0,1);
                    }
                }

                for (int j = 0; j < _PointCount; j++) 
                {

                    // define points
                    float3 sphericalPointPos = _PointSphericalCoords[j].xyz;
                    
                    // geodesic distance
                    float geodesicDistance = GetGeodesicDistance(sphericalPointPos, sphericalWorldPos);
                    
                    // spherical distance
                    float distanceKarlsruhe = GetSphericalDistance(sphericalPointPos, sphericalWorldPos);

                    if (_MetricType == 0 || _MetricType == 2) {
                        if (_ClosestDistance == 1 ? geodesicDistance < refDistEuclid : geodesicDistance > refDistEuclid)
                        {
                            refDistEuclid = geodesicDistance;
                            finalColorIndexEuclid = j;
                        }
                    } 
                    
                    if (_MetricType == 1 || _MetricType == 2) {
                        if (_ClosestDistance == 1 ? distanceKarlsruhe < refDistKarlsruhe : distanceKarlsruhe > refDistKarlsruhe)
                        {
                            refDistKarlsruhe = distanceKarlsruhe;
                            finalColorIndexKarlsruhe = j;
                        }
                    } 
                }

                float4 color = float4(0,0,0,1);

                if (_MetricType == 0) 
                {
                    if (_MaxDistancePercentage == 0 || refDistEuclid <= (UNITY_PI * _Radius) * (_MaxDistancePercentage / 100)) 
                    {
                        color = _Colors[finalColorIndexEuclid];
                    }
                }
                else if (_MetricType == 1) 
                {
                    if (_MaxDistancePercentage == 0 || refDistKarlsruhe <= (UNITY_PI * _Radius) * (_MaxDistancePercentage / 100)) 
                    {
                        color = _Colors[finalColorIndexKarlsruhe];
                    }
                }
                else if (_MetricType == 2) 
                {
                    if ((finalColorIndexEuclid == finalColorIndexKarlsruhe) && (_MaxDistancePercentage == 0 || refDistKarlsruhe <= (UNITY_PI * _Radius) * (_MaxDistancePercentage / 100))) 
                    {
                        color = _Colors[finalColorIndexKarlsruhe];
                    }
                }
                return applyGrid(sphericalWorldPos, color);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
