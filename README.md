## ErrorCorrection

A library to implement Reed-Solomon encoding. Reed Solomon is a method of encoding data with extra
error correction information built in, so that errors in received data can be corrected without having
to retransmit the data; this technique is also known as Forward Error Correction (FEC). FEC finds uses
in situations where reacquisition of the data is impossible or impractical, such as stored data on a 
hard disk or transmitted over a wireless protocol.

Some readers may be familiar with the idea of parity - some value computed from the rest of the data that
is transmitted along with the data. RS is similar to the idea in parity, but different in what sort of 
properties it has. For instance, one common application of the idea of parity is in Raid-5 harddrive 
systems. In the simplest case, you have three drives in total; one drive stores one half of the original 
data, the second drive stores the other half of the original data, and the third drive stores the XOR
of the two original drives - A XOR B = C. Since the XOR operation is bijective, you can recover any one 
part from the two remaining parts. If drive A fails, you can compute `A = B XOR C` to get your data back.
You can extend this scheme to more than three drives, however, you can only have one set of parity data.
Say you had five drives - then your parity would be `E = A XOR B XOR C XOR D`. A problem arises 
however - there's less redundency. You can only lose 1 out of any 5 drives, any more and you've lost everything.
You might be tempted to say "well, why not mirror the parity drive?". That does buy a little more redundency, but
it's not as good as it could be. For instance, if you lose the two parity drives, you don't lose any data. 
But if you lose two original data drives, you've lost everything - with mirror parity drives, you can't lose
*any* two drives. In the lingo, "Mirrored XOR" is not a *maximally-separated* encoding.

Reed-Solomon provides a way to implement this system - with Reed-Solomon, you can actually tune to your 
heart's content how much parity to have, whether it be 1/3 of the stored or transmitted data, 1/2, 5/6ths - 
the choice is yours. This is what makes RS and other FEC schemes so powerful.

...

Reed-Solomon encoding is built on the mathematics of finite fields, also known as Galois fields. 
Blocks of bytes to transmit are formed as polynomials over a finite field; the error correction bytes
are formed by dividing the message polynomial by a code generator polynomial; the remainder of that division
becomes the error correction bytes.

## Implementation Notes

This implementation performs the Reed-Solomon transformations over arrays of ints, as it seems to be the 
fastest-performing means to do so. This has implications for the maximum size of an encoded block. 

In RS, the size of a symbol (and thus the number of independent symbols) is linked to the size of the 
encoded block. If there are 16 symbols, then the whole block is limited to 15 places, with each place
being one of 16 values. Such a system would require that the entire transmitted block be exactly 60 
bits long. Similarly for a system of 256 symbols, the entire block is limited to 255 places (for a total
block size of 255*256 = 65280 bits ).

Since this library stores symbols in ints, the maximum symbol can only be 32 bits, and thus, the maximum
block size is 2^32 * (2^32 -1).

There is however, a much more pertinent limit to the size of blocks in this library. In order to quickly
compute multiplication, the library uses a look-up table. The size of this table is the square of the 
number of elements in the field. For a field of 256 elements (an 8-bit field), the size of the largest 
look-up table is 256 * 256 * 32 bits == 2 MiB. For a 9-bit field of 512 elements, the size of the look-up
table is 8 MiB.

None of these limits affect the total number of blocks you can transmit; message sizes can be arbitrarily long.

This library does provide a set of stream-based adapters, allowing users to read and write normal files as 
standard streams. 


## License ##

This code is licensed under the [BSD 2-clause license][1].

[1]: https://opensource.org/licenses/BSD-2-Clause
