document.getElementById('barcodeForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const dataVencimento = document.getElementById('dataVencimento').value;
    const valor = parseFloat(document.getElementById('valor').value);

    try {
        const response = await fetch('http://localhost:7071/api/barcode-generate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                dataVencimento,
                valor
            })
        });

        if (!response.ok) {
            throw new Error('Erro ao gerar o boleto');
        }

        const data = await response.json();
        
        // Exibir o resultado
        document.getElementById('result').classList.remove('hidden');
        document.getElementById('barcodeImage').src = `data:image/png;base64,${data.imagemBase64}`;
        document.getElementById('barcodeText').textContent = data.barcode;
        const barcodeText = document.getElementById('barcodeText');
        barcodeText.style.color = '';
        barcodeText.style.backgroundColor = '#f8f9fa';
        // Exibir botão de validação
        const validateBtn = document.getElementById('validateBtn');
        validateBtn.classList.remove('hidden');
        validateBtn.onclick = async function() {
            try {
                const resp = await fetch('http://localhost:7072/api/barcode_validate', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ barcode: data.barcode })
                });
                if (!resp.ok) throw new Error('Erro ao validar o código de barras');
                const valid = await resp.json();
                if (valid.valido === true) {
                    barcodeText.style.color = '#fff';
                    barcodeText.style.backgroundColor = '#27ae60';
                } else {
                    barcodeText.style.color = '';
                    barcodeText.style.backgroundColor = '#e74c3c';
                }
            } catch (err) {
                alert('Erro ao validar: ' + err.message);
            }
        };
    } catch (error) {
        alert('Erro ao gerar o boleto: ' + error.message);
    }
});
