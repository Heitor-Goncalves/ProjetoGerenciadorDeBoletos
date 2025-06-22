# Serviço Autenticador de Boletos - Azure Function

Este projeto é uma **Azure Function desenvolvida em .NET 8 (Isolated Worker Model)**, com integração ao **Azure Service Bus**, **BarcodeLib** e **Newtonsoft.Json**.

---

## 📌 Objetivo

O serviço tem como principal objetivo **gerar e validar boletos bancários**, utilizando filas do Azure Service Bus para processamentos assíncronos e escaláveis.

---

## 🛠️ Tecnologias utilizadas

- .NET 8 (Isolated Worker)
- Azure Functions v4
- Azure Service Bus (Trigger e Sender)
- BarcodeLib (Geração de código de barras)
- Newtonsoft.Json (Serialização/Deserialização JSON)

---

## 🚀 Funcionalidades principais

### ✅ Funções disponíveis:

#### 📄 `geradorDeBoletos`

Responsável por **gerar códigos de barras de boletos** a partir dos dados recebidos via HTTP.  
Utiliza a biblioteca **BarcodeLib** para criar imagens de código de barras.

---

#### 📄 `validadorDeBoletos`

Responsável por **validar se o código de barras de um boleto é válido** de acordo com as regras criadas

