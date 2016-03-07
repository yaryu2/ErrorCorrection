## ErrorCorrection

A library to implement Reed-Solomon encoding. Reed Solomon is a method of encoding data with extra
error correction information built in, so that errors in received data can be corrected without having
to retransmit the data; this technique is also known as Forward Error Correction (FEC). FEC finds uses
in situations where reacquisition of the data is impossible or impractical, such as stored data on a 
hard disk or transmitted over a wireless protocol.

Reed-Solomon encoding is built on the mathematics of finite fields, also known as Galois fields. 
Blocks of bytes to transmit are formed as polynomials over a finite field; the error correction bytes
are formed by dividing the message polynomial by a code generator polynomial ith the remainer polynomial 
becomes the error correction bytes.

## Implementation Notes

This implementation performs the Reed-Solomon transformations over arrays of ints, as it seems to be the 
fastest-performing means to do so. This has implications for the maximum size of an encoded message. 

In RS, the size of a symbol (and thus the number of independent symbols) is linked to the size of the 
encoded message. If there are 16 symbols, then the whole message is limited to 15 places, with each place
being one of 16 values. Such a system would require that the entire transmitted message be exactly 60 
bits long. Similarly for a system of 256 symbols, the entire message is limited to 255 places (for a total
message size of 255*256 = 65280 bits ).

Since this library stores symbols in ints, the maximum symbol can only be 32 bits, and thus, the maximum
message size is 2^32 * (2^32 -1).

There is however, a much more pertinent limit to the size of messages in this library. In order to quickly
compute multiplication, the library uses a look-up table. The size of this table is the square of the 
number of elements in the field. For a field of 256 elements (an 8-bit field), the size of the largest 
look-up table is 256 * 256 * 32 bits == 2 MiB. For a 9-bit field of 512 elements, the size of the look-up
table is 8 MiB.

This library does provide a set of stream-based adapters, allowing users to read and write normal files as 
standard streams. 
