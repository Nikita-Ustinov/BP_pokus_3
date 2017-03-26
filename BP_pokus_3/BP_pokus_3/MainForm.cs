using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace BP_pokus_3
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	partial class MainForm : Form
	{	static String _info;
		static String  _convolutionsInfo;
		int Answer;
		bool [] UzByli = new bool[] {false, false, false, false, false, false , false, false, false, false};				//,
		Neuronet net ;
		double Max;
		
		public  void newEpoch() {
			for (int i=0; i<10; i++) {
				UzByli[i] = false;
			}
		}
		
		public MainForm()
		{
			InitializeComponent();
		}
		public MainForm(int i) {}
		
		public  double[,] getPicture() {
			Random rand = new Random();
			int a;
			do {
				a = rand.Next(0, 10);
			} while (UzByli[a] == true);
			String fileName = a.ToString()+".jpeg";
			UzByli[a] = true;
			Answer = a;
			return ImgToRightPicture(fileName);
		}
		
		double[,] ImgToRightPicture(String file) {
			Image img = Image.FromFile(file);
			Bitmap bm = new Bitmap(img);
			Color color =new Color();
			double[,] vysledek = new double[39,30];
			
			for (int i=0;i<39; i++) {
				for (int j=0; j<30; j++) {
					color = bm.GetPixel(j,i);
					vysledek[i,j] = color.GetBrightness();
				}
				if(i==29) {
					
				}
			}
			return vysledek;
		}
		
		int calculateResult(double[,] picture) {
			writeInfo(picture, "start");
			double[][,] firstConvolution = new double[5][,];				//prvni vrstva, convolution 11x11
			for(int i=0; i<5; i++) {
				firstConvolution[i] = applyConvolution(i, picture);							//prvni konvoluce
				if (i==0)
					writeInfo(firstConvolution[i], "apply convolution 1");
				firstConvolution[i] = addX(firstConvolution[i]);				
				if (i==0)
					writeInfo(firstConvolution[i], "Add x - 0");
				firstConvolution[i] = pooling(2, firstConvolution[i]);						//prvni pooling
				if (i==0)
					writeInfo(firstConvolution[i], "pooling 1");
				firstConvolution[i] = function(firstConvolution[i]);						// prvni funkce aktivace (Tanh)
				if (i==0)
					writeInfo(firstConvolution[i], "TanH 1");
			}
			
			double[][,] secondConvolution = new double[25][,];
			int  cisloFiltra = 5;
			int  cisloPolozky = 0;
			for(int j=0; j<5; j++) {
				for(int i=0; i<5; i++) {
					secondConvolution[cisloPolozky] = applyConvolution(cisloFiltra, firstConvolution[j]);
					if (i==0)
						writeInfo(picture, "apply convolution 2");
					secondConvolution[cisloPolozky] = pooling(2, secondConvolution[cisloPolozky]);
					if (i==0)
						writeInfo(picture, "poolin 2");
					secondConvolution[cisloPolozky] = function(secondConvolution[cisloPolozky]);
					cisloFiltra++;
					cisloPolozky++;
				}
				cisloFiltra = 5;
			}
			double[][,] thirdConvolution = new double[125][,];
			cisloFiltra = 10;
			cisloPolozky = 0;
			for(int j=0; j<25; j++) {
				for(int i=0; i<5; i++) {
					thirdConvolution[cisloPolozky] = applyConvolution(cisloFiltra, secondConvolution[j]);
					writeInfo(picture, "apply convolution 3");
					thirdConvolution[cisloPolozky] = function(thirdConvolution[cisloPolozky]);
					cisloFiltra++;
					cisloPolozky++;
				}
				cisloFiltra = 10;
			}
			double[] inputFullyConnectionNet = doOneArray(thirdConvolution);
			net.l1.writeInput(inputFullyConnectionNet);									//zapisuje vstupni vektor v "fully connected" neuronovou sit
			net.l1.countOutputs();
			Neuron templ1 = net.l2.head;
			for (int i=0; i<Neuronet.druhaVrstva; i++){									//zapisuje do druhe vrstvy FC neuronove siti vstupni signaly
				for (int j=0; j<Neuronet.prvniVrstva; j++){
					templ1.input[j]=net.l1.outputs[j];
				}
				templ1=templ1.next;
			}
			net.l2.countOutputs();
			templ1=net.l3.head;
			for (int i=0; i<Neuronet.tretiVrstva; i++){									// zapisuje do treti vrstvy FC neuronove siti vstupni signaly
				for (int j=0; j<Neuronet.druhaVrstva; j++){
					templ1.input[j]=net.l2.outputs[j];
				}
				templ1=templ1.next;
			}
			net.l3.countOutputs();
			int index = 0;																//cislo neuronu ktery vyhral
			Max = net.l3.outputs[0];
			for (int i=0; i<net.l3.outputs.Length; i++) {
				if (Max<net.l3.outputs[i])
				{
					Max = net.l3.outputs[i];
					index = i;
				}
			}
			LinkedListNode<Convolution> templ = net.convolutions.First;
			for (int i=0; i<15; i++) {
				templ.Value.countAverageInput();
				templ.Value.countAverageOutput();
			}
			
			return index;
		}
		
		double[,] function(double[,] picture) {
			double[,] result = picture;
			for(int i=0; i<picture.GetUpperBound(0)+1; i++) {
				for(int j=0; j<picture.GetUpperBound(1)+1; j++) {
					result[i,j] = 1.7159*Math.Tanh(0.66*picture[i,j]);
				}
			}
			return result;
		}
		
		void study() {
			double [] err = new double[Neuronet.tretiVrstva];
			int iteration = 0;
			double lokError = 0;
			int lokResult = 0;
			double errorMin = 100;
			
			while(test() < 100) {
				lokResult = calculateResult(getPicture());
				Neuron templ3 = net.l3.head;
				for  (int i=0; i<Neuronet.tretiVrstva; i++) {
					if (Answer == i)
						err[i] = Max - templ3.output ;									//zapisuje signal chyby vystupni vrstvy
					else
						err[i] = 0 - templ3.output;
					templ3 = templ3.next;
				}
				Neuron templ2;
				templ3 = net.l3.head;
				for (int i=0; i<Neuronet.tretiVrstva; i++) {
					templ2 = net.l2.head;
					templ3.grad = 0.388*(1.7159 - templ3.output)*(1.7159 + templ3.output)*err[i];				//pocita gradient pro vystupni vyrstvu
					for (int j=0; j<Neuronet.druhaVrstva; j++) {
						templ3.weights[j]+=net.speedL*templ2.output*templ3.grad;								//pocita vahy pro vystupni vrstvu
						templ2 = templ2.next;
					}
					templ3 = templ3.next;
				}
				
				double grad = 0;
				Neuron templ1;
				templ2 = net.l2.head;
				for (int i=0; i<Neuronet.druhaVrstva; i++) {
					grad = 0;
					templ3 = net.l3.head;
					for(int u=0; u<Neuronet.tretiVrstva; u++) {						//sumarizuje gradient predhozi vrstvy (delta pravidlo pro druhou vrstvu)
						grad+=templ3.grad*templ3.weights[i];
						templ3 = templ3.next;
					}
					templ2.grad = grad*0.388*(1.7159-templ2.output)*(1.7159+ templ2.output);
					templ1 = net.l1.head;
					for (int j=0; j<Neuronet.prvniVrstva; j++) {
						templ2.weights[j]+=net.speedL*templ1.output*templ2.grad;
						templ1 = templ1.next;
					}
					templ2 = templ2.next;
				}
				
				templ1 = net.l1.head;
				for (int i=0; i<Neuronet.prvniVrstva; i++) {
					grad = 0;
					templ2 = net.l2.head;
					for(int u=0; u<Neuronet.druhaVrstva; u++) {							//sumarizuje gradient predhozi vrstvy (delta pravidlo pro prvni vrstvu)
						grad+=templ2.grad*templ2.weights[i];
						templ2 = templ2.next;
					}
					templ1.grad = grad*0.388*(1.7159-templ1.output)*(1.7159+ templ1.output);
					for (int j=0; j<Neuronet.inputLength; j++) {
						templ1.weights[j]+=net.speedL*templ1.input[j]*grad;
					}
					templ1 = templ1.next;
				}
				
				//pro filtry 10 az 14
				LinkedListNode<Convolution> templ = net.convolutions.First;
				while(templ.Value.cisloFiltra!=10) {
					templ = templ.Next;
				}
				for(int i=0; i<5; i++) {
					grad = 0;
					templ1 = net.l1.head;												//vrstva se ktere scita gradienty
					for(int j=0; j<net.l1.length; j++) {
						grad+=templ1.grad;												//sumarizuje gradient predhozi vrstvy
						templ1 = templ1.next;
					}
					templ.Value.grad = grad*0.388*(1.7159-templ.Value.averageOutput)*(1.7159+templ.Value.averageOutput)/net.l1.length;
					for(int k=0; k<templ.Value.weights.GetUpperBound(0)+1; k++) {
						for(int q=0; q<templ.Value.weights.GetUpperBound(1)+1; q++) {
							templ.Value.weights[k,q] += net.speedL*templ.Value.grad*templ.Value.avInput[k,q].average;
						}
					}
					templ = templ.Next;
				}
				
				//pro filtry 5 az 9
				templ = net.convolutions.First;
				while(templ.Value.cisloFiltra!=5) {
					templ = templ.Next;
				}
				for(int i=0; i<5; i++) {
					grad = 0;
					LinkedListNode<Convolution> templLast = net.convolutions.First;					//vrstva se ktere scita gradienty
					while(templLast.Value.cisloFiltra != 10) {
						templLast = templLast.Next;
					}
					for(int j=0; j<5; j++) {
						grad += templLast.Value.grad;												//sumarizuje gradient predhozi vrstvy
						templLast = templLast.Next;
					}
					templ.Value.grad = grad*0.388*(1.7159-templ.Value.averageOutput)*(1.7159+templ.Value.averageOutput)/Neuronet.prvniVrstva;
					for(int k=0; k<templ.Value.weights.GetUpperBound(0)+1; k++) {
						for(int q=0; q<templ.Value.weights.GetUpperBound(1)+1; q++) {
							templ.Value.weights[k,q] += net.speedL*templ.Value.grad*templ.Value.avInput[k,q].average;
						}
					}
					templ = templ.Next;
				}
				
				//pro filtry 0 az 4
				templ = net.convolutions.First;
				for(int i=0; i<5; i++) {
					grad = 0;
					LinkedListNode<Convolution> templLast = net.convolutions.First;					//vrstva se ktere scita gradienty
					while(templLast.Value.cisloFiltra != 5) {
						templLast = templLast.Next;
					}
					for(int j=0; j<5; j++) {
						grad += templLast.Value.grad;												//sumarizuje gradient predhozi vrstvy
						templLast = templLast.Next;
					}
					templ.Value.grad = grad*0.388*(1.7159-templ.Value.averageOutput)*(1.7159+templ.Value.averageOutput)/Neuronet.druhaVrstva;
					for(int k=0; k<templ.Value.weights.GetUpperBound(0)+1; k++) {
						for(int q=0; q<templ.Value.weights.GetUpperBound(1)+1; q++) {
							templ.Value.weights[k,q] += net.speedL*templ.Value.grad*templ.Value.avInput[k,q].average;
						}
					}
					templ = templ.Next;
				}
				iteration++;
				
				if (Answer != lokResult)
					lokError++;
				
				if ((lokError/iteration*100)<errorMin)
					errorMin = lokError/iteration*100;
			}
			
		}
		
		double[,] pooling(int size, double[,] picture) {             			// size treba 2x2 => size=2
			double[,] result = new double[(picture.GetUpperBound(0)+1)/size,(picture.GetUpperBound(1)+1)/size];
			int x0, x1, y0, y1;
			y0 = 0;
			y1 = size;
			for(int i=0; i<result.GetUpperBound(0)+1; i++) {
				x0 = 0;
				x1 = size;
				for(int j=0; j<result.GetUpperBound(1)+1; j++) {
					result[i,j] = max(picture, x0, x1, y0, y1);
					x0+=size;
					x1+=size;
				}
				x0 = 0;
				x1 = size;
				y0+=size;
				y1+=size;
			}
			return result;
		}
		
		double max(double[,] picture, int x0, int x1, int y0, int y1) {
			double result = picture[y0,x0];
			for(int i=y0; i<y1; i++) {
				for(int j=x0; j< x1; j++) {
					if(picture[i,j]>result)
						result = picture[i,j];
				}
			}
			return result;
		}
		
		double[,] applyConvolution(int cisloFiltra, double[,] picture){
			LinkedListNode<Convolution> templ = net.convolutions.First;
			while(cisloFiltra != 0 ) {
				templ = templ.Next;
				cisloFiltra--;
			}
			int x = picture.GetUpperBound(1)-templ.Value.weights.GetUpperBound(1)+2;		// rozmer vysledne matici - x a y
			int y = picture.GetUpperBound(0)-templ.Value.weights.GetUpperBound(0)+2;
			double[,] result = new double[y,x];
			int x0, y0;
			for(int i=0; i<y; i++) {
				x0 = 0;
				y0 = 0;
				for(int j=0; j<x; j++) {
					result[i,j] = sum(picture,templ.Value, x0, y0);
					x0++;
				}
				y0++;
			}
			return result;
		}

		double sum(double[,] picture, Convolution templ , int x0, int y0) {
			double result = 0;
			int y = 0;			// kountery pro konvoluce
			int x = 0;
			for(int i=y0; i<y0+templ.size; i++) {
				for(int j=x0; j<x0+templ.size-1; j++) {
					result += picture[i,j]*templ.weights[y,x];
					x++;
				}
				x=0;
				y++;
			}
			templ.allOutputs.AddLast(result);
			return result;
		}

		void Button1Click(object sender, EventArgs e)
		{
			net = new Neuronet();
			study();
		}

		public static void writeInfo(double[,] picture, String typeOfTransformation)
		{
			_info +=typeOfTransformation+" -> "+"\r\n"+"x- "+ (picture.GetUpperBound(1)+1).ToString() +"\r\n"+ "y -"+(picture.GetUpperBound(0)+1).ToString()+"\r\n";
			File.WriteAllText("Date.txt", _info);
			writeConvolution(picture, typeOfTransformation);
		}

		static void writeConvolution(double[,] picture, string typeOfTransformation)
		{	
			_convolutionsInfo += typeOfTransformation+"\r\n";
			for (int i=0; i<picture.GetUpperBound(0)+1; i++) {
				for (int j=0; j<picture.GetUpperBound(1)+1; j++) {
					_convolutionsInfo += picture[i,j].ToString()+"  ";
				}
				_convolutionsInfo += "\r\n";
			}
			_convolutionsInfo += "\r\n"+"\r\n";
			File.WriteAllText("Convolutions.txt", _convolutionsInfo);
		}
			
		double[] doOneArray(double[][,] thirdConvolution) {
		int length = thirdConvolution.Length*thirdConvolution[1].Length;
		double[] result = new double[length];
		int counter = 0;
			for (int i = 0; i<thirdConvolution.Length; i++) {
				for(int j=0; j<thirdConvolution[i].GetUpperBound(0); j++) {
					for(int k=0; k< thirdConvolution[i].GetUpperBound(1); k++) {
						result[counter] = thirdConvolution[i][j,k];
						counter++;
					}
				}
			}
			return result;
		}
		
		public	int test() {
			int vysledek=0;
			for (int j=0; j<10; j++) {
				newEpoch();
				for (int i=0; i<10; i++) {
					int vysOperace = calculateResult(getPicture());
					if (Answer == vysOperace) {
						vysledek += 1;
					}
					else
						vysledek += 0;
				}
			}
			return vysledek;
		}

		void serializace() {
			BinaryFormatter formatter = new BinaryFormatter();
			using ( var fSream = new FileStream("weights.dat", FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(fSream, net);
			}
		}

		Neuronet deseralizace() {
			try {
				using (var fStream = File.OpenRead("weights.dat")) {
					BinaryFormatter formatter = new BinaryFormatter();
					return  (Neuronet)formatter.Deserialize(fStream);
				}
			} catch {
				return new Neuronet();
			}
		}
		
		double[,] addX(double[,] picture) {								//pridani sloupce '0' k polu
			double[,] result = new double[picture.GetUpperBound(0)+1, picture.GetUpperBound(1)+2 ];
			for(int i=0; i<picture.GetUpperBound(0)+1; i++) {
				for(int j=0; j<picture.GetUpperBound(1)+2; j++) {
					if(j==picture.GetUpperBound(1)+1) {
						result[i,j] = 0;
					}
					else {
						result[i,j] = picture[i,j];
					}
				}
			}
			return result;
		}
//
//		double[,] addY(double[,] picture) {									//pridani radka '0' k polu
//			double[,] result = new double[picture.GetUpperBound(0)+2, picture.GetUpperBound(1)+1 ];
//			for (int i=0; i<picture.GetUpperBound(1)+1; i++) {
//				result[picture.GetUpperBound(0)+2, i] = 0;
//			}
//			return result;
//		}
		
	}
}
