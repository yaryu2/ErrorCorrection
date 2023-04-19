using ErrorCorrectionRS;

int dataLength = 10; 
int parityLength = 4;
int totalLength = dataLength + parityLength; 


var data = new int[dataLength];
for (int i = 0; i < dataLength; i++)
{
    data[i] = (byte)(i + 1); 
}

var codeword = new int[totalLength];

Encoder en = new Encoder(totalLength, dataLength, parityLength, 2);
Decoder dec = new Decoder(totalLength, dataLength, parityLength, 2);
en.Encode(data);

// Simulate some errors in the codeword
codeword[2] = 0x00; // Example error: changing one byte to 0x00
codeword[7] = 0xFF; // Example error: changing one byte to 0xFF

// Decode the codeword using Reed-Solomon
dec.Decode(codeword);

// Check if decoding was successful and print the results
// if (success)
// {
//     Console.WriteLine("Decoding successful!");
//     Console.WriteLine("Decoded Data: ");
//     for (int i = 0; i < dataLength; i++)
//     {
//         Console.Write(decodedData[i] + " ");
//     }
// }
// else
// {
//     Console.WriteLine("Decoding failed!");
// }