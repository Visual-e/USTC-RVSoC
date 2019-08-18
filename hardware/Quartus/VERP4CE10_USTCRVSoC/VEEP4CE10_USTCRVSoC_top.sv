module VEEP4CE10_USTCRVSoC_top(
    //////////// CLOCK //////////
    input  clock_50,
    //////////// LED, KEY, Switch //////////
    output [7:0] led,
    //////////// VGA //////////
    output vga_hsync,
	 output vga_vsync,
	 output vga_red,
	 output vga_green,
	 output vga_blue,
	 /////////// USART //////////
	 input  isp_uart_rx,
	 output isp_uart_tx
);

soc_top soc_inst(
    .clk              ( clock_50      ),
    .isp_uart_rx      ( isp_uart_rx   ),
    .isp_uart_tx      ( isp_uart_tx   ),
    .vga_hsync        ( vga_hsync     ),
    .vga_vsync        ( vga_vsync     ),
    .vga_red          ( vga_red       ),
    .vga_green        ( vga_green     ),
    .vga_blue         ( vga_blue      )
);

// Индикатор работы SOC
reg [21:0] cnt = 22'h0;
reg [ 5:0] flow = 6'h0;
always @ (posedge clock_50) begin
    cnt <= cnt + 22'h1;
    if(cnt==22'h0)
        flow <= {flow[4:0], ~flow[5]};
end
    
assign led[5:0] = flow;

endmodule
