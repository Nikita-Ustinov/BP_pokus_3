using System;
using System.Collections.Generic;

namespace BP_pokus_3
{																																												/// <summary> /// Description of Neuronet. /// </summary>
	public class Neuronet
	{	
		public List l1, l2, l3;
		public static int inputLength = 250;		//pro fully connection neuronet
		public static int prvniVrstva = 50;		
		public static int druhaVrstva= 50;		
		public static int tretiVrstva= 10;
		public double speedL = 0.1;
		
		public LinkedList<Convolution> convolutions = new LinkedList<Convolution>();
		
		public Neuronet()
		{
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			
			addFilter(5);					//pridani konvoluci 5x5   
			addFilter(5);					//pridani konvoluci 5x5
			addFilter(5);					//pridani konvoluci 5x5
			addFilter(5);					//pridani konvoluci 5x5
			addFilter(5);					//pridani konvoluci 5x5
			
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			
			l1= new List(0);				//create first fully connected layer 
			l2= new List(1);				//create second fully connected layer 
			l3= new List(2);				//create output fully connected layer 
		}
		
		void addFilter(int size) {
			int cisloFiltra = 0;
			if(convolutions.First==null) {
				convolutions.AddLast(new Convolution(size,cisloFiltra));
			} 
			else {
//				cisloFiltra = 1;
				LinkedListNode<Convolution> templ = convolutions.First;
				while(templ!=null) {
					templ = templ.Next;
					cisloFiltra++;
				}
				convolutions.AddLast(new Convolution(size, cisloFiltra));
			}
				
		}
	}
}
