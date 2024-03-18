# Introduction 
TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project. 

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

The ImageProcessingOrchestrator starts by calling the InconsistencyAnalysis function and waits for the result.
After receiving the inconsistencyAnalysisResult, it checks if the analysis passed. If not, it sets the failure reason and returns.
If the inconsistency analysis passed, it calls the ContentExtraction function and waits for the result.
After receiving the contentExtractionResult, it calls the TransactionLookup function and waits for the result.
After receiving the transactionLookupResult, it checks if the transaction was found. If not, it sets the failure reason and returns.
If the transaction was found, it calls the SignatureAnalysis function and waits for the result.
After receiving the signatureAnalysisResult, it checks if the signature is valid. If not, it sets the failure reason and returns.
If the signature is valid, it sets the success status and the process ends.
If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)

DevOps PAT:
jfohgokchfan334fup7iauwvozxpedz2pjvos67bxnsnbtqwyoaa

SWa0amWLrE7YwuyGxuHSJCIlIEStscCGi/8EVe9nA5j4PigdIOliKtae8e/0BIfgZkVT29uevisw+ASteG/Olw==
