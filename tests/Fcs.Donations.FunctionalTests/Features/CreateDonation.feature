Funcionalidade: Criar doacao

  Cenario: Criar doacao valida
    Dado que eu possuo uma requisicao valida de doacao
    E a campanha esta elegivel
    Quando eu executar o caso de uso de criacao
    Entao a doacao deve ser criada com sucesso
    E uma mensagem de outbox deve ser gerada
