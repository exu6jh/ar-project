//UNITY_SHADER_NO_UPGRADE
#ifndef  MYHLSLINCLUDE_INCLUDED
#define  MYHLSLINCLUDE_INCLUDED

void MapClamp_float(float RawVal, float RawMin, float RawMax, out float ClampVal)
{
    ClampVal = saturate((RawVal - RawMin) / (RawMax - RawMin));
}

void MapClamp_half(half RawVal, half RawMin, half RawMax, out half ClampVal)
{
    ClampVal = saturate((RawVal - RawMin) / (RawMax - RawMin));
}

void MapClampMax_float(float RawVal, float RawMax, out float ClampVal)
{
    ClampVal = saturate( RawVal / RawMax );
}

void MapClampMax_half(half RawVal, half RawMax, out half ClampVal)
{
    ClampVal = saturate( RawVal / RawMax );
}

void InvertMapClampMax_float(float RawVal, float RawMax, out float ClampVal)
{
    ClampVal = 1 - saturate( RawVal / RawMax );
}

void InvertMapClampMax_half(half RawVal, half RawMax, out half ClampVal)
{
    ClampVal = 1 - saturate( RawVal / RawMax );
}

void InvertQuadMapClampMax_float(float RawVal, float RawMax, out float ClampVal)
{
    ClampVal = 1 - saturate( pow(RawVal / RawMax, 2) );
}

void InvertQuadMapClampMax_half(half RawVal, half RawMax, out half ClampVal)
{
    ClampVal = 1 - saturate( pow(RawVal / RawMax, 2) );
}



void FocusPositionTest_float(float FocusDis, float3 CameraPos, float3 CameraDir, out float3 FocusPos)
{
    FocusPos = CameraPos + FocusDis * CameraDir;
}

void FocusPositionTest_half(half FocusDis, half3 CameraPos, half3 CameraDir, out half3 FocusPos)
{
    FocusPos = CameraPos + FocusDis * CameraDir;
}

#endif //MYHLSLINCLUDE_INCLUDED