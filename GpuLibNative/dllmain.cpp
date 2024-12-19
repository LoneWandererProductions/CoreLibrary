#include "pch.h"
#include <d3d11.h>
#include <d3dcompiler.h>
#include <vector>
#include <stdexcept>
#include <wrl.h>

// Use Microsoft::WRL::ComPtr for automatic resource management
using Microsoft::WRL::ComPtr;

extern "C" __declspec(dllexport)
void UpdatePixels(int* pixels, int width, int height, int color, const int* indices, int numIndices)
{
    try
    {
        // Initialize Direct3D device and context
        ComPtr<ID3D11Device> device;
        ComPtr<ID3D11DeviceContext> context;
        HRESULT hr = D3D11CreateDevice(
            nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, 0, nullptr, 0,
            D3D11_SDK_VERSION, &device, nullptr, &context);
        if (FAILED(hr)) throw std::runtime_error("Failed to create D3D11 device.");

        // Compile Compute Shader
        ComPtr<ID3DBlob> shaderBlob;
        hr = D3DCompileFromFile(
            L"MinimalComputeShader.hlsl", nullptr, nullptr, "CSMain", "cs_5_0",
            0, 0, &shaderBlob, nullptr);
        if (FAILED(hr)) throw std::runtime_error("Failed to compile compute shader.");

        ComPtr<ID3D11ComputeShader> computeShader;
        hr = device->CreateComputeShader(
            shaderBlob->GetBufferPointer(), shaderBlob->GetBufferSize(), nullptr, &computeShader);
        if (FAILED(hr)) throw std::runtime_error("Failed to create compute shader.");

        // Create 2D Texture
        D3D11_TEXTURE2D_DESC textureDesc = {};
        textureDesc.Width = width;
        textureDesc.Height = height;
        textureDesc.MipLevels = 1;
        textureDesc.ArraySize = 1;
        textureDesc.Format = DXGI_FORMAT_R32_SINT;
        textureDesc.Usage = D3D11_USAGE_DEFAULT;
        textureDesc.BindFlags = D3D11_BIND_UNORDERED_ACCESS;
        textureDesc.CPUAccessFlags = 0;

        ComPtr<ID3D11Texture2D> texture;
        hr = device->CreateTexture2D(&textureDesc, nullptr, &texture);
        if (FAILED(hr)) throw std::runtime_error("Failed to create 2D texture.");

        // Create UAV
        ComPtr<ID3D11UnorderedAccessView> uav;
        hr = device->CreateUnorderedAccessView(texture.Get(), nullptr, &uav);
        if (FAILED(hr)) throw std::runtime_error("Failed to create UAV.");

        // Set up the Compute Shader
        context->CSSetShader(computeShader.Get(), nullptr, 0);
        context->CSSetUnorderedAccessViews(0, 1, uav.GetAddressOf(), nullptr);

        // Dispatch Compute Shader
        context->Dispatch((width + 15) / 16, (height + 15) / 16, 1);

        // If you need to copy back results to CPU:
        // 1. Create a staging texture with CPU_READ access.
        // 2. Copy the updated texture to the staging texture.
        // 3. Map the staging texture to read the updated pixel data.
    }
    catch (const std::exception& ex)
    {
        // Log or handle errors
        printf("Error: %s\n", ex.what());
    }
}
