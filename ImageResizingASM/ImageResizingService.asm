
COMMENT !
	; Parametry wej�ciowe: byte[] source, byte[] target, int w1, int w2, int widthRatio, int heightRatio, int y

	int y2 = (y * heightRatio) >> 16;
	for (int j = w2 - 1; j >= 0 ; j--)
	{
		int x2 = (j * widthRatio) >> 16;

		int sourceIndex = ((y2 * w1) + x2) >> 2;
		int targetIndex = ((y * w2) + j) >> 2;

		targetArray[targetIndex] = initArray[sourceIndex];
		targetArray[targetIndex + 1] = initArray[sourceIndex + 1];
		targetArray[targetIndex + 2] = initArray[sourceIndex + 2];
		targetArray[targetIndex + 3] = initArray[sourceIndex + 3];
	}
!

.data
	w1 QWORD ?
	w2 QWORD ?
	h1 QWORD ?
	h2 QWORD ?
	widthRatio QWORD ?
	heightRatio QWORD ?

.code

SetParameters proc ; int w1, int w2, int h1, int h2

	PUSH R12
	PUSH R13
	PUSH R14
	PUSH R15

	mov R13, 10h

	mov w1, RCX ; RCX zawiera warto�� w1
	mov w2, RDX ; RDX zawiera warto�� w2

	mov h1, R8 ; R8 zawiera warto�� h1
	mov h2, R9 ; R9 zawiera warto�� h2

	 ; int widthRatio = ((w1 << 16) / w2) + 1;
	movq MM0, w1 ; przesuwamy warto�� w1 do rejestru MM0

	movq MM4, R13 ; przesuwamy warto�� R13 <=> 10h czyli (16)10, co odpowiada przesuni�ciu o 16 bit�w
	PSLLD MM0, MM4 ; przesuni�cie liczby 32 bitowej o 16 bit�w w lewo (warto�ci znajduj�cej si� w MM4)

	movq MM4, w2 ; do MM4 wprowadzamy warto�� w2

	; division goes here
	cvtpi2ps XMM0, MM0 ; Zamiana warto�ci MM0, na liczb� zmienno przecinkow� i zapisanie w rejestrze XMM0
	cvtpi2ps XMM1, MM4 ; Zamiana warto�ci MM4, na liczb� zmienno przecinkow� i zapisanie w rejestrze XMM1

	divss XMM0, XMM1 ; Teraz mo�emy dokona� dzielenia, wynik l�duje w XMM0

	cvtps2pi MM0, XMM0 ; Zamiana liczby zmienno przecinkowej na integer i zapis do MM0 (z zaokr�gleniem) dlatego inc nie musi by� dodatkowo wywo�ywane
	
	movq widthRatio, MM0 ; Przeniesienie warto��i rejestr MM0, do zmiennej globalnej widthRatio

     ; int heightRatio = ((h1 << 16) / h2) + 1;
	movq MM0, h1 ; przesuwamy warto�� h1 do rejestru MM0

	movq MM4, R13 ; przesuwamy warto�� R13 <=> 10h czyli (16)10, co odpowiada przesuni�ciu o 16 bit�w 
	PSLLD MM0, MM4 ; przesuni�cie liczby 32 bitowej o 16 bit�w w lewo (warto�ci znajduj�cej si� w MM4)

	movq MM4, h2 ; do MM4 wprowadzamy warto�� h2

	cvtpi2ps XMM0, MM0 ; Zamiana warto�ci MM0, na liczb� zmienno przecinkow� i zapisanie w rejestrze XMM0
	cvtpi2ps XMM1, MM4 ; Zamiana warto�ci MM4, na liczb� zmienno przecinkow� i zapisanie w rejestrze XMM1

	divss XMM0, XMM1 ; Teraz mo�emy dokona� dzielenia, wynik l�duje w XMM0

	cvtps2pi MM0, XMM0 ; Zamiana liczby zmienno przecinkowej na integer i zapis do MM0 (z zaokr�gleniem) dlatego inc nie musi by� dodatkowo wywo�ywane

	movq heightRatio, MM0 ; Przeniesienie warto��i rejestr MM0, do zmiennej globalnej heightRatio

	POP R15
	POP R14
	POP R13
	POP R12
	ret

SetParameters endp

MyProc proc

	PUSH R12
	PUSH R13
	PUSH R14
	PUSH R15

	; RCX - byte[] source
	; RDX - byte[] target
	; R8 - int y
	mov R15, R8

	mov R13, 10h
	mov R12, 02h

	; int y2 = (y * heightRatio) >> 16; MM6
	; MM4 odgrywa rol� tymczasowego rejestru

	movq MM6, R15
	movq MM4, heightRatio

	PMULUDQ MM6, MM4 ; Mno�enie zawarto�ci rejestr�w MM6 i MM4, zapis do MM6

	movq MM4, R13 ; przesuwamy warto�� R13 <=> 10h czyli (16)10, co odpowiada przesuni�ciu o 16 bit�w 
	PSRAD MM6, MM4 ; przesuni�cie zawarto�ci rejestru MM6, 16 bit�w w prawo; MM6 zawiera poprawnie wyliczon� warto�� y2

	mov R14, w2 ; definicja licznika b�tli, tyle iteracji ile pixeli w rz�dzi
start_loop:
	dec R14

	; int x2 = (j * widthRatio) >> 16;
	; MM1 - x2

	movq MM1, R14 ; wprowadzenie warto�ci j do rejestru MM1
	movq MM4, widthRatio ; wprowadzenie warto�ci widthRatio do rejestru tymczasowego MM4

	PMULUDQ MM1, MM4 ; wymno�enie warto�ci rejestr�w MM1 i MM4 (j * widthRatio) i zapis do rejestru MM1
	
	movq MM4, R13 ; przesuwamy warto�� R13 <=> 10h czyli (16)10, co odpowiada przesuni�ciu o 16 bit�w 
	PSRAD MM1, MM4 ; przesuni�cie warto�ci rejestru MM1 o 16 bit�w w prawo; w MM1 znajduje si� docelowa warto�� x1


	; int sourceIndex = ((y2 * w1) + x2) << 2;
	; MM2 - source index
	movq MM2, MM6 ; przesuni�cie warto�ci y2 (MM6) do rejestru MM2
	movq MM4, w1 ; przesuni�cie w1 do tymczasowego rejestru

	PMULUDQ MM2, MM4 ; mno�enie warto�ci y2 * w1 i zapis do rejestru MM2
	paddd MM2, MM1 ; dodanie warto�ci x2 (MM1) i zapis do rejestru MM2

	movq MM5, R12
	PSLLD MM2, MM5

	; int targetIndex = ((y * w2) + j) << 2;
	; MM3 - target index

	movq MM3, R15 ; przesuni�cie do rejetru warto�ci y (R15)
	movq MM4, w2 ; przesuni�cie w2 do tymczasowego rejestru

	PMULUDQ MM3, MM4 ; wymno�enie warto�ci rejestr�w MM3 (y) i MM4(w2) i zapis do rejestru MM3

	movq MM4, R14 ; wprowadzenie do rejestru MM4 warto�ci j (R14)
	paddd MM3, MM4 ; dodanie zawarto�ci rejestr�w MM3 i MM4 z zapisem do MM3 (y * w2) + j

	PSLLD MM3, MM5 ; przesini�cie zawarto�ci rejestra MM3 o 2 w lewo (<< 2)


	movq R10, MM2 ; zapis warto�ci sourceIndex (MM2) do rejestru R10
	movq R11, MM3 ; zapis warto�ci targetIndex (MM3) do rejestru R11

	mov EAX, [RCX + R10] ; zapis do rejestru MM0 4 byt�w (quadword) z adresu RCX (initArray) + sourceIndex
	mov [RDX + R11], EAX ; zapis do kom�rki pami�ci pod adresem RDX (targetArray) + targetIndex, z warto�ci rejestru MM0

	CMP R14, 00
	JNZ start_loop

	POP R15
	POP R14
	POP R13
	POP R12
	ret

MyProc endp

end
