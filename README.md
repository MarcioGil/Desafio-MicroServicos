# Desafio Técnico: Plataforma de E-commerce com Microserviços

## 1. Visão Geral do Projeto

Este projeto implementa uma solução de **microserviços** para uma plataforma de e-commerce, focada no gerenciamento de **Estoque de Produtos** e **Gestão de Vendas**. A arquitetura foi desenhada para ser escalável, robusta e com separação clara de responsabilidades, conforme as boas práticas de arquitetura de software.

### 1.1. Arquitetura Proposta

A solução é composta por quatro serviços principais, orquestrados via Docker Compose para facilitar o desenvolvimento e a implantação:

| Serviço | Tecnologia | Responsabilidade | Comunicação | Porta Padrão |
| :--- | :--- | :--- | :--- | :--- |
| **AuthService** | .NET 8 Web API | Geração de tokens JWT (Mock) | HTTP | 5003 |
| **StockService** | .NET 8 Web API | CRUD de produtos e controle de estoque | HTTP (API Gateway) e RabbitMQ (Consumidor) | 5001 |
| **SalesService** | .NET 8 Web API | Criação e consulta de pedidos de venda | HTTP (API Gateway) e RabbitMQ (Produtor) | 5002 |
| **ApiGateway** | Ocelot | Roteamento de requisições e autenticação JWT | HTTP | 5000 |
| **RabbitMQ** | RabbitMQ (Docker) | Broker de mensagens para comunicação assíncrona | AMQP | 5672 (15672 Management) |

### 1.2. Tecnologias Utilizadas

*   **Backend:** .NET 8 (C#)
*   **Banco de Dados:** Entity Framework Core (com In-Memory Database para simplicidade e portabilidade)
*   **Comunicação Assíncrona:** RabbitMQ
*   **API Gateway:** Ocelot
*   **Autenticação:** JWT (JSON Web Tokens)
*   **Orquestração:** Docker e Docker Compose

## 2. Configuração e Execução

### 2.1. Pré-requisitos

Para executar o projeto, você precisa ter instalado:

*   [**Docker**](https://docs.docker.com/get-docker/)
*   [**Docker Compose**](https://docs.docker.com/compose/install/)

### 2.2. Passos para Execução

1.  **Clone o repositório:**
    \`\`\`bash
    git clone [URL_DO_REPOSITÓRIO]
    cd Desafio-MicroServicos
    \`\`\`

2.  **Inicie os serviços com Docker Compose:**
    O arquivo `docker-compose.yml` está configurado para construir as imagens dos microserviços e iniciar o RabbitMQ.

    \`\`\`bash
    docker-compose up --build
    \`\`\`

3.  **Acesse o sistema:**
    *   **API Gateway:** \`http://localhost:5000\`
    *   **RabbitMQ Management:** \`http://localhost:15672\` (Usuário: `user`, Senha: `password`)

    Os endpoints dos microserviços são acessíveis através do API Gateway.

## 3. Detalhamento dos Microserviços

### 3.1. AuthService (Porta 5003)

Serviço responsável por gerar o token JWT necessário para acessar os demais endpoints protegidos.

| Endpoint | Método | Descrição | Proteção |
| :--- | :--- | :--- | :--- |
| `/api/auth/login` | POST | Gera um token JWT de mock. | Nenhuma |

**Exemplo de uso (via API Gateway):**

\`\`\`bash
# 1. Obter o Token de Autenticação (usando um mock user/password)
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{"username":"admin", "password":"password"}' | jq -r '.token')
echo $TOKEN
\`\`\`

### 3.2. StockService (Porta 5001)

Gerencia o cadastro e o estoque de produtos.

| Endpoint (via Gateway) | Método | Descrição | Proteção |
| :--- | :--- | :--- | :--- |
| `/api/stock/products` | GET | Lista todos os produtos em estoque. | Nenhuma (Anon.) |
| `/api/stock/products/{id}` | GET | Consulta um produto específico. | Nenhuma (Anon.) |
| `/api/stock/products` | POST | Cadastra um novo produto. | JWT |
| `/api/stock/products/stock-update` | PUT | **Endpoint interno** para atualização de estoque (usado pelo RabbitMQ). | JWT |

**Funcionalidade Assíncrona:**

*   **Consumidor:** O serviço possui um `OrderCreatedConsumer` que escuta a fila `stock_update_queue` no RabbitMQ. Ao receber uma mensagem de `order.created`, ele processa a lista de itens do pedido e reduz o estoque de cada produto de forma assíncrona.

### 3.3. SalesService (Porta 5002)

Gerencia a criação e consulta de pedidos de venda.

| Endpoint (via Gateway) | Método | Descrição | Proteção |
| :--- | :--- | :--- | :--- |
| `/api/sales/orders` | POST | Cria um novo pedido de venda. | JWT |
| `/api/sales/orders` | GET | Lista todos os pedidos. | JWT |
| `/api/sales/orders/{id}` | GET | Consulta um pedido específico. | JWT |

**Funcionalidade Assíncrona:**

*   **Produtor:** Ao receber uma requisição de `POST /api/sales/orders`, o serviço salva o pedido no banco de dados com o status `Pending` e imediatamente publica uma mensagem de `order.created` no RabbitMQ para que o StockService realize a baixa do estoque.

## 4. Comunicação Assíncrona (RabbitMQ)

A comunicação entre o **SalesService** e o **StockService** é feita de forma assíncrona utilizando o RabbitMQ, implementando o padrão **Event-Driven Architecture (EDA)**.

*   **Evento:** `order.created`
*   **Produtor:** SalesService (Publica o evento após a criação do pedido).
*   **Consumidor:** StockService (Consome o evento e realiza a baixa de estoque).

Esta abordagem garante que a criação do pedido seja rápida (não bloqueando a resposta HTTP) e que a atualização de estoque ocorra de forma resiliente, mesmo que o StockService esteja temporariamente indisponível.

## 5. Segurança (JWT e API Gateway)

### 5.1. Autenticação JWT

A autenticação é implementada via JWT, com a chave de assinatura configurada nos arquivos `appsettings.json` de cada serviço.

*   O **AuthService** gera o token.
*   O **ApiGateway** e os microserviços **StockService** e **SalesService** são configurados para validar este token.

### 5.2. API Gateway (Ocelot)

O Ocelot é utilizado para centralizar o acesso e aplicar a autenticação.

*   **Roteamento:** Todas as requisições externas são direcionadas ao `http://localhost:5000` e o Ocelot as roteia para as portas internas corretas (5001, 5002, 5003).
*   **Proteção:** As rotas que exigem autenticação (ex: `POST /api/stock/products`, `POST /api/sales/orders`) têm a política de autenticação JWT aplicada diretamente no arquivo `ocelot.json`.

## 6. Testes Unitários e Qualidade

O projeto inclui uma pasta de testes (`src/MicroservicesEcommerce.Tests`) com testes unitários para as principais funcionalidades do núcleo de negócio:

*   **StockServiceTests:** Testa a adição de produtos, a consulta e, crucialmente, a lógica de atualização de estoque, incluindo o cenário de estoque insuficiente.
*   **SalesServiceTests:** Testa a criação de pedidos e a atualização de status.

**Para executar os testes:**

\`\`\`bash
# Certifique-se de estar na raiz do projeto
cd /home/ubuntu/microservices-ecommerce
dotnet test src/MicroservicesEcommerce.Tests/MicroservicesEcommerce.Tests.csproj
\`\`\`

## 7. Boas Práticas e Escalabilidade

### 7.1. Boas Práticas de Código

*   **Separação de Responsabilidades (SRP):** Cada serviço (Estoque, Vendas, Autenticação) tem uma única responsabilidade. Dentro dos serviços, a lógica de negócio está separada em *Services* e a camada de acesso a dados/controllers em *Data* e *Controllers*.
*   **RESTful API:** Uso de verbos HTTP e códigos de status apropriados (200 OK, 201 Created, 400 Bad Request, 404 Not Found).
*   **Padrão de Design:** Uso de DTOs (Data Transfer Objects) para entrada de dados.

### 7.2. Escalabilidade

*   **Stateless Services:** Todos os microserviços são *stateless* (sem estado em memória), o que permite escalabilidade horizontal. Basta aumentar o número de réplicas no `docker-compose.yml` para escalar.
*   **Comunicação Assíncrona:** O RabbitMQ desacopla o SalesService do StockService, permitindo que a falha em um não derrube o outro e que o StockService possa processar a baixa de estoque em seu próprio ritmo.
*   **Configuração via Ambiente:** As configurações de conexão com RabbitMQ e as chaves JWT são lidas de variáveis de ambiente no `docker-compose.yml`, facilitando a implantação em ambientes de produção (Kubernetes, ECS, etc.).

## 8. Logs e Monitoramento

A configuração de *logging* básica está presente nos arquivos `appsettings.json` de cada serviço, utilizando o *logging* nativo do .NET Core.

*   **Nível de Log:** O nível de log padrão está configurado para `Information`, o que permite rastrear o fluxo de execução sem poluir os logs.
*   **Logs de Erro:** Qualquer falha na baixa de estoque (ex: estoque insuficiente) é explicitamente logada no console do StockService, permitindo o rastreamento de transações críticas.

Em um ambiente de produção, os logs seriam centralizados por um sistema como ELK (Elasticsearch, Logstash, Kibana) ou Grafana Loki. O uso do Docker Compose já facilita a coleta de logs de todos os serviços em um único ponto.

---
**Autor:** Manus AI
**Data:** 28 de Outubro de 2025
**Repositório:** [https://github.com/MarcioGil/Desafio-MicroServicos.git](https://github.com/MarcioGil/Desafio-MicroServicos.git)
\`\`\`
