# 🏬 Desafio Técnico – Microserviços E-commerce

Este projeto implementa uma plataforma de e-commerce distribuída utilizando arquitetura de microserviços. A solução contempla controle de estoque, gestão de pedidos e comunicação assíncrona entre serviços, respeitando boas práticas de escalabilidade, segurança e resiliência em sistemas modernos.

A proposta reflete um ambiente real de negócios, contribuindo para uma compreensão prática de soluções robustas e flexíveis, voltadas à melhoria da vida das pessoas através da tecnologia.

---

## 🧩 Arquitetura da Solução

- **StockService**: gestão de produtos e atualização de estoque
- **SalesService**: criação e consulta de pedidos
- **AuthService**: autenticação JWT
- **API Gateway**: roteamento e segurança centralizada
- **RabbitMQ**: mensageria assíncrona entre microserviços

### 🔷 Diagrama da Arquitetura

```mermaid
flowchart LR
    User --> |HTTP/HTTPS| ApiGateway
    ApiGateway --> |JWT| AuthService
    ApiGateway --> StockService
    ApiGateway --> SalesService
    SalesService -->|Publica evento order.created| RabbitMQ
    RabbitMQ -->|Consumidor| StockService
    StockService --> DB1[(Banco de Dados Estoque)]
    SalesService --> DB2[(Banco de Dados Pedidos)]
🛠 Tecnologias Utilizadas
Componente	Tecnologia
Linguagem	.NET 8 (C#)
Banco de Dados	Entity Framework Core (In-Memory)
Mensageria	RabbitMQ
Security	JWT
API Gateway	Ocelot
Deploy Dev	Docker + Docker Compose

🚀 Execução do Projeto
✅ Pré-requisitos
Docker

Docker Compose

▶️ Como rodar:
bash
Copiar código
git clone https://github.com/MarcioGil/Desafio-MicroServicos.git
cd Desafio-MicroServicos
docker-compose up --build
🔗 Endpoints principais
Serviço	URL
API Gateway	http://localhost:5000
RabbitMQ Management	http://localhost:15672 (user: user / password: password)

🔐 Autenticação (JWT)
Obter Token
bash
Copiar código
curl -X POST http://localhost:5000/api/auth/login \
-H "Content-Type: application/json" \
-d '{"username":"admin","password":"password"}'
Salve o token retornado e use nas próximas chamadas:

bash
Copiar código
-H "Authorization: Bearer SEU_TOKEN_AQUI"
📦 StockService (Estoque)
Endpoint	Método	Proteção	Descrição
/api/stock/products	GET	Nenhuma	Lista produtos
/api/stock/products/{id}	GET	Nenhuma	Detalhes
/api/stock/products	POST	JWT	Cadastrar produto

📨 Consumidor de RabbitMQ:

Evento: order.created

Ação: redução de estoque

🧾 SalesService (Vendas)
Endpoint	Método	Proteção	Descrição
/api/sales/orders	POST	JWT	Criar pedido
/api/sales/orders	GET	JWT	Consultar pedidos

📡 Produtor de RabbitMQ:

Publica evento ao criar pedido gerando atualização posterior no estoque

📊 Logs e Monitoramento
Logs configurados para nível Information

Erros de estoque insuficiente são rastreados no StockService

Preparado para integração futura com ELK, Grafana, Loki etc.

🧪 Testes Unitários
bash
Copiar código
cd src/MicroservicesEcommerce.Tests
dotnet test
Testes incluem:

Lógica de validação de estoque

Criação e consulta de pedidos

🌱 Propósito e Evolução
O projeto foi construído com foco no aprendizado sólido e significativo, valorizando:

Desacoplamento de responsabilidades

Comunicação resiliente entre serviços

Segurança como pilar essencial

Design orientado ao impacto social

Novos serviços (Pagamentos, Entregas, Analytics) podem ser adicionados sem atrito.

✨ Autor
Márcio Alexandre de Paiva Gil
Estudante de Engenharia de Software
Turma 14 – DIO Campus Expert
Apaixonado por educação, tecnologia, inovação e justiça social 🤝🚀

🔗 Conecte-se comigo:

LinkedIn: https://linkedin.com/in/márcio-gil-1b7669309

Portfólio: https://marciogil.github.io/curriculum-vitae/

Repositório deste projeto: https://github.com/MarcioGil/Desafio-MicroServicos.git

📌 Data
Atualizado em 28 de Outubro de 2025

yaml
Copiar código

---

## ✅ Próximo passo

Se quiser, posso:

✅ Criar também o **diagrama visual de mensagens** (RabbitMQ)  
✅ Melhorar o estilo com badges do GitHub: CI, Docker, .NET, RabbitMQ  
✅ Adicionar **collection do Postman** exportável  
✅ Criar **exemplos completos com payload** (body JSON) para cada endpoint  
✅ Organizar labels, issues e roadmap no GitHub

Quer que eu já mande a **coleção Postman** junto com payloads corretos para teste?  
Se quiser, é só dizer **“Sim, quero os testes”** ✅ 😉

Gostou? Quer ajustar alguma parte?  
Estou com você até publicar e brilhar com esse projeto! 🚀💙